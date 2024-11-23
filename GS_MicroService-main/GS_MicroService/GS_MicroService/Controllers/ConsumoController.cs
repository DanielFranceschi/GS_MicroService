using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ConsumoController : ControllerBase
{
    private readonly IMongoCollection<ConsumoModel> _consumos;

    public ConsumoController(IMongoClient mongoClient, IConfiguration configuration)
    {
        var databaseName = configuration.GetSection("MongoDB:DatabaseName").Value;
        var collectionName = configuration.GetSection("MongoDB:CollectionName").Value;

        if (string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(collectionName))
        {
            throw new ArgumentException("Configurações do MongoDB não estão completas.");
        }

        var database = mongoClient.GetDatabase(databaseName);
        _consumos = database.GetCollection<ConsumoModel>(collectionName);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterConsumo([FromBody] ConsumoModel consumo)
    {
        if (consumo == null)
        {
            return BadRequest(new { message = "Dados inválidos" });
        }

        try
        {
            // Remover o ID enviado pelo cliente, pois será gerado automaticamente pelo MongoDB
            consumo.Id = null;

            // Configurar a data de registro automaticamente
            consumo.DataRegistro = DateTime.UtcNow;

            // Validar o valor de consumo e gerar um valor aleatório caso não seja fornecido ou seja inválido
            if (consumo.Consumo <= 0)
            {
                Random random = new Random();
                consumo.Consumo = random.Next(1, 100); // Gera um valor entre 1 e 100
            }

            await _consumos.InsertOneAsync(consumo);

            // Retornar a resposta com o ID do novo consumo
            return CreatedAtAction(nameof(GetConsumoById), new { id = consumo.Id }, consumo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao registrar o consumo", error = ex.Message });
        }
    }

    [HttpGet("{id:length(24)}")]
    public async Task<IActionResult> GetConsumoById(string id)
    {
        try
        {
            var consumo = await _consumos.Find(c => c.Id == id).FirstOrDefaultAsync();

            if (consumo == null)
            {
                return NotFound(new { message = "Registro não encontrado" });
            }

            return Ok(consumo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar o consumo", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetConsumos()
    {
        try
        {
            var consumos = await _consumos.Find(_ => true).ToListAsync();

            if (consumos.Count == 0)
            {
                return NotFound(new { message = "Nenhum registro encontrado" });
            }

            return Ok(consumos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao buscar os consumos", error = ex.Message });
        }
    }

    // Método adicional para atualizar o consumo (caso seja necessário)
    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> UpdateConsumo(string id, [FromBody] ConsumoModel consumo)
    {
        if (consumo == null || id != consumo.Id)
        {
            return BadRequest(new { message = "Dados inválidos ou IDs não coincidem" });
        }

        try
        {
            var existingConsumo = await _consumos.Find(c => c.Id == id).FirstOrDefaultAsync();

            if (existingConsumo == null)
            {
                return NotFound(new { message = "Consumo não encontrado" });
            }

            // Atualiza os dados necessários, por exemplo, o valor de consumo
            existingConsumo.Consumo = consumo.Consumo > 0 ? consumo.Consumo : existingConsumo.Consumo;
            existingConsumo.DataRegistro = DateTime.UtcNow; // Atualiza a data de registro

            await _consumos.ReplaceOneAsync(c => c.Id == id, existingConsumo);

            return Ok(existingConsumo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao atualizar o consumo", error = ex.Message });
        }
    }

    // Método adicional para excluir um consumo
    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> DeleteConsumo(string id)
    {
        try
        {
            var result = await _consumos.DeleteOneAsync(c => c.Id == id);

            if (result.DeletedCount == 0)
            {
                return NotFound(new { message = "Consumo não encontrado para exclusão" });
            }

            return NoContent(); // Retorna 204 sem conteúdo
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao excluir o consumo", error = ex.Message });
        }
    }
}
