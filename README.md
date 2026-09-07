# UNIVERSIDADE VILA VELHA
# DESENVOLVIMENTO WEB BACK-END
# Sistema de Gestão de Consultas

## Desenvolvido por Arturo Cabral Figna | Matrícula: 202529837

Aplicação Web desenvolvida em C#, ASP.NET Core 8 (MVC) para a disciplina de
Desenvolvimento Web Back-end. O sistema permite o cadastro de usuários, autenticação
e o gerenciamento completo de consultas médicas ou profissionais.

## Vídeo demonstrativo

Link: https://youtu.be/Mkesbzl6V9U

O vídeo demonstra o cadastro de usuário, o login e o registro de consultas.

## Tecnologias utilizadas

Linguagem: C# 12
Framework: ASP.NET Core 8.0 (MVC)
ORM: Entity Framework Core 8.0 (abordagem Code First)
Banco de dados: SQL Server 2022 Express
Autenticação: Cookie Authentication + `PasswordHasher`
Interface: Razor Views + Bootstrap 5

## requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Express, Developer ou LocalDB)
- Ferramenta de linha de comando do EF Core:

```bash
dotnet tool install --global dotnet-ef --version 8.*
```

## Configuração do banco de dados

### 1. Ajustar a Connection String

A string de conexão fica em [`appsettings.json`](appsettings.json), em `ConnectionStrings`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=UvvConsultasDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

Ajuste o valor de `Server=` conforme a sua instalação:

| Sua instalação | Valor de `Server=` |
|----------------|--------------------|
| SQL Server Express | `.\SQLEXPRESS` |
| LocalDB | `(localdb)\MSSQLLocalDB` |
| Instância padrão local | `localhost` |

### 2. Criar o banco a partir das Migrations

o EF Core gera o banco a partir das Migrations.

Pelo Package Manager Console:

```powershell
Update-Database
```

Pela linha de comando:

```bash
dotnet ef database update
```

Ao final, o banco `UvvConsultasDb` terá as tabelas `Usuarios` e `Consultas`.

> Para recriar as migrations do zero:
> `dotnet ef migrations add InitialCreate` seguido de `dotnet ef database update`.

## Executando a aplicação

```bash
dotnet run
```

A aplicação sobe em `https://localhost:xxxx`

## Funcionalidades

| Funcionalidade | Rota | Verbo HTTP |
|---|---|---|
| Cadastro de usuário | `/Conta/Registrar` | `GET` / `POST` |
| Login | `/Conta/Login` | `GET` / `POST` |
| Logout | `/Conta/Logout` | `POST` |
| Listar consultas | `/Consultas` | `GET` |
| Detalhes da consulta | `/Consultas/Details/{id}` | `GET` |
| Cadastrar consulta | `/Consultas/Create` | `GET` / `POST` |
| Editar consulta | `/Consultas/Edit/{id}` | `GET` / `POST` |
| Excluir consulta | `/Consultas/Delete/{id}` | `GET` / `POST` |

## Estrutura do projeto

```
GestaoConsultasUVV/
├── Controllers/
│   ├── ContaController.cs        # Cadastro, login e logout
│   ├── ConsultasController.cs    # CRUD de consultas ([Authorize])
│   └── HomeController.cs
├── Data/
│   └── AppDbContext.cs           # DbContext do EF Core
├── Models/
│   ├── Usuario.cs                # Entidade Usuario
│   └── Consulta.cs               # Entidade Consulta (1:N com Usuario)
├── ViewModels/
│   ├── RegistroViewModel.cs      # Entrada do formulário de cadastro
│   └── LoginViewModel.cs         # Entrada do formulário de login
├── Views/
│   ├── Conta/                    # Registrar, Login
│   ├── Consultas/                # Index, Create, Edit, Delete, Details
│   ├── Home/
│   └── Shared/                   # _Layout com menu condicional
├── Migrations/                   # Migrations geradas pelo EF Core
├── appsettings.json              # Connection String
└── Program.cs                    # DI + pipeline de middlewares
```

## Modelo de dados

```
Usuario                          Consulta

Id           (PK)                Id             (PK)
Nome                             Especialidade
Email        (índice único)      DataHora
SenhaHash                        Descricao
DataCadastro                     UsuarioId      (FK -> Usuario.Id)
```

Relacionamento 1:N, um usuário possui várias consultas
(`OnDelete: Cascade`).

## Decisões de arquitetura e segurança

### Injeção de Dependência

O `DbContext` e o hash são registrados no container em `Program.cs`:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
```

### Ordem do pipeline de middlewares

`UseAuthentication()` vem obrigatoriamente antes de `UseAuthorization()` —
primeiro o sistema identifica quem é o usuário, depois decide o que ele pode fazer:

```csharp
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
```

### Proteção de rotas

O `ConsultasController` usa `[Authorize]` na classe inteira. Visitantes não
autenticados são redirecionados para `/Conta/Login`.

### Senhas

A senha não é armazenada em texto puro. O `PasswordHasher<Usuario>` gera o hash e apenas o campo `SenhaHash` persiste.

### Isolamento entre usuários

Todas as consultas do EF Core filtram por `UsuarioId == UsuarioLogadoId`. Isso impede
que um usuário acesse, edite ou exclua a consulta de outro apenas trocando o `id` na URL.

### Validação

Validação no servidor através do Data Annotations (`[Required]`, `[EmailAddress]`,
`[StringLength]`, `[Compare]`) e no cliente via `_ValidationScriptsPartial`.
Todos os formulários `POST` usam `[ValidateAntiForgeryToken]` contra CSRF.
