using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using TrilhaNetAzureDesafio.Context;
using TrilhaNetAzureDesafio.Models;

namespace TrilhaNetAzureDesafio.Controllers;

[ApiController]
[Route("[controller]")]
public class FuncionarioController : ControllerBase
{
    private readonly RHContext _context;
    private readonly string _connectionString;
    private readonly string _tableName;

    public FuncionarioController(RHContext context, IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.GetValue<string>("ConnectionStrings:SAConnectionString");
        _tableName = configuration.GetValue<string>("ConnectionStrings:AzureTableName");
    }

    private TableClient GetTableClient()
    {
        var serviceClient = new TableServiceClient(_connectionString);
        var tableClient = serviceClient.GetTableClient(_tableName);

        tableClient.CreateIfNotExists();
        return tableClient;
    }

    private void  CriarLog (Funcionario  funcionario, TipoAcao tipo)
    {
        string guid = Guid.NewGuid().ToString();

        var tableClient = GetTableClient();
        var funcionarioLog = new FuncionarioLog(funcionario, tipo, guid , guid);

        tableClient.UpsertEntity(funcionarioLog);
    }

    [HttpGet("obter/{id}")]
    public IActionResult ObterPorId(int id)
    {
        var funcionario = _context.Funcionarios.Find(id);

        if (funcionario == null)
            return NotFound();

        return Ok(funcionario);
    }

    [HttpPost("criar")]
    public IActionResult Criar(Funcionario funcionario)
    {
        _context.Funcionarios.Add(funcionario);
        _context.SaveChanges();
             
        CriarLog(funcionario, TipoAcao.Inclusao);

        return CreatedAtAction(nameof(ObterPorId), new { id = funcionario.Id }, funcionario);
    }

    [HttpPut("atualizar/{id}")]
    public IActionResult Atualizar(int id, Funcionario funcionario)
    {
        var funcionarioBanco = _context.Funcionarios.Find(id);

        if (funcionarioBanco == null)
            return NotFound();

        funcionarioBanco.Nome = funcionario.Nome;
        funcionarioBanco.Endereco = funcionario.Endereco;
        funcionarioBanco.Ramal = funcionario.Ramal;
        funcionarioBanco.EmailProfissional = funcionario.EmailProfissional;
        funcionarioBanco.Departamento = funcionario.Departamento;
        funcionarioBanco.Salario = funcionario.Salario;
        funcionarioBanco.DataAdmissao = funcionario.DataAdmissao;

        _context.Update(funcionarioBanco);
        _context.SaveChanges();
        CriarLog(funcionarioBanco, TipoAcao.Atualizacao);

        return Ok(funcionarioBanco);
    }

    [HttpDelete("deletar/{id}")]
    public IActionResult Deletar(int id)
    {
        var funcionarioBanco = _context.Funcionarios.Find(id);

        if (funcionarioBanco == null)
            return NotFound();

        _context.Funcionarios.Remove(funcionarioBanco);
        _context.SaveChanges();

        var tableClient = GetTableClient();
        CriarLog(funcionarioBanco, TipoAcao.Remocao);

        return NoContent();
    }
}
