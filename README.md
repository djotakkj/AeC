# AeC — Gerenciador de Endereços

Aplicação web desenvolvida em **ASP.NET MVC (.NET 10)** como teste prático para a vaga de Desenvolvedor C# na AeC.

## Funcionalidades

- **Login** com autenticação por cookie e senha criptografada (BCrypt)
- **CRUD completo** de endereços por usuário
- **Busca automática por CEP** via integração com a API pública [ViaCEP](https://viacep.com.br/)
- **Exportação para CSV** dos endereços cadastrados
- Isolamento de dados por usuário (cada usuário vê apenas seus próprios endereços)

## Tecnologias

| Camada       | Tecnologia                          |
|--------------|-------------------------------------|
| Backend      | ASP.NET MVC, C#, .NET 10            |
| ORM          | Entity Framework Core               |
| Banco        | SQL Server                          |
| Autenticação | Cookie Authentication + BCrypt      |
| Frontend     | HTML, CSS customizado, Bootstrap 5  |
| API externa  | ViaCEP (busca de endereço por CEP)  |

## Como executar

### Pré-requisitos
- .NET 10 SDK
- SQL Server (local ou remoto)

### Passos

1. Clone o repositório:
   ```bash
   git clone <url-do-repositorio>
   cd AeC.Enderecos
   ```

2. Configure a connection string em `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=AeCEnderecos;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

3. Execute as migrations para criar o banco:
   ```bash
   dotnet ef database update
   ```

4. Rode a aplicação:
   ```bash
   dotnet run
   ```

5. Acesse `https://localhost:5001` e faça login com:
   - **Usuário:** `admin`
   - **Senha:** `123456`

> O usuário padrão é criado automaticamente na primeira execução.

## Estrutura do projeto

```
AeC.Enderecos/
├── Controllers/
│   ├── AuthController.cs        # Login e logout
│   └── EnderecosController.cs   # CRUD + busca CEP + exportação CSV
├── Data/
│   └── AppDbContext.cs          # Contexto do Entity Framework
├── Migrations/                  # Migrations geradas pelo EF Core
├── Models/
│   ├── Endereco.cs              # Modelo de endereço
│   ├── Usuario.cs               # Modelo de usuário
│   └── ViaCepResponse.cs        # DTO de resposta da API ViaCEP
├── Services/
│   └── ViaCepService.cs         # Integração com a API ViaCEP
├── Views/
│   ├── Auth/Login.cshtml        # Tela de login
│   └── Enderecos/               # Index, Create, Edit, Delete, Details
├── wwwroot/css/site.css         # Estilos customizados
├── database_scripts.sql         # Scripts SQL de criação das tabelas
└── Program.cs                   # Configuração da aplicação
```

## Scripts SQL

Os scripts de criação das tabelas estão em [`database_scripts.sql`](database_scripts.sql).
