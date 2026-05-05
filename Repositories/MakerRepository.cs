using Microsoft.Data.SqlClient;
using MakersApi.DTOs;
using MakersApi.Exceptions;

namespace MakersApi.Repositories;

public class MakerRepository : IMakerRepository
{
    private readonly string _connectionString;

    public MakerRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("no connection string");
    }

    public async Task<MakerDetailsResponse?> GetByIdAsync(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();


        const string sql = @"
            SELECT  m.Id            AS MakerId,
                    m.Name          AS MakerName,
                    p.Id            AS ProductId,
                    p.Name          AS ProductName,
                    p.Description,
                    p.StickerPrice,
                    pt.Id           AS ProductTypeId,
                    pt.Name         AS ProductTypeName,
                    v.Code          AS VendorCode,
                    v.Name          AS VendorName,
                    vp.Amount,
                    vp.PricePerUnit
            FROM    Makers m
            LEFT JOIN Products       p  ON p.MakerId       = m.Id
            LEFT JOIN ProductTypes   pt ON pt.Id           = p.ProductTypeId
            LEFT JOIN VendorProducts vp ON vp.ProductId    = p.Id
            LEFT JOIN Vendors        v  ON v.Code          = vp.VendorCode
            WHERE   m.Id = @id
            ORDER BY p.Id, v.Code;";




        await using var cmd = new SqlCommand(sql, connection); 
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync();

        MakerDetailsResponse? maker = null;
        var productsById = new Dictionary<int, MakerProductResponse>();

        while (await reader.ReadAsync())
        {
            if (maker is null)
            {
                maker = new MakerDetailsResponse
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Products = new List<MakerProductResponse>()
                };
            }

            if (reader.IsDBNull(2))
                continue;

            var productId = reader.GetInt32(2);
            if (!productsById.TryGetValue(productId, out var product))
            {
                product = new MakerProductResponse
                {
                    Id = productId,
                    Name = reader.GetString(3),
                    Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                    StrickerPrice = reader.GetDecimal(5),
                    ProductType = new ProductTypeResponse
                    {
                        Id = reader.GetInt32(6),
                        Name = reader.GetString(7)
                    },
                    Vendors = new List<ProductVendorResponse>()
                };
                productsById.Add(productId, product);
                maker.Products.Add(product);
            }

            if (reader.IsDBNull(8))
                continue;

            product.Vendors.Add(new ProductVendorResponse
            {
                Code = reader.GetString(8),
                Name = reader.GetString(9),
                Amount = reader.GetInt32(10),
                PricePerUnit = reader.GetDecimal(11)
            });
        }

        return maker;
    }

    public async Task CreateAsync(CreateMakerRequest request)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            var typeIds = new Dictionary<string, int>();
            if (request.Products is not null)
            {
                foreach (var product in request.Products)
                {
                    if (typeIds.ContainsKey(product.Type))
                        continue;

                    await using var cmd = new SqlCommand(
                        "SELECT Id FROM ProductTypes WHERE Name = @name;",
                        connection, transaction);

                    cmd.Parameters.AddWithValue("@name", product.Type);

                    var result = await cmd.ExecuteScalarAsync();
                    if (result is null)
                        throw new NotFoundException($"product type '{product.Type}' isnt found");

                    typeIds[product.Type] = (int)result;
                }
            }

            int makerId;
            await using (var cmd = new SqlCommand(
                "INSERT INTO Makers (Name) OUTPUT INSERTED.Id VALUES (@name);",
                connection, transaction))
            {
                cmd.Parameters.AddWithValue("@name", request.Name);
                makerId = (int)(await cmd.ExecuteScalarAsync())!;
            }

            if (request.Products is not null)
            {
                foreach (var product in request.Products)
                {
                    await using var cmd = new SqlCommand(@"
                        INSERT INTO Products (Name, Description, StickerPrice, ProductTypeId, MakerId)
                        VALUES (@name, @description, @stickerPrice, @productTypeId, @makerId);",
                        connection, transaction);
                    cmd.Parameters.AddWithValue("@name", product.Name);
                    cmd.Parameters.AddWithValue("@description",
                        (object?)product.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@stickerPrice", product.StrickerPrice);
                    cmd.Parameters.AddWithValue("@productTypeId", typeIds[product.Type]);
                    cmd.Parameters.AddWithValue("@makerId", makerId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
