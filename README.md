# 🎮 GameLog - Backend

Esse repositório tem como objetivo servir como backend do projeto GameLog, utilizado para avaliação na disciplina de Engenharia de Software da turma CP700TIN3 na instituição FACENS.

## 🏗️ Arquitetura

### Clean Architecture
O backend utiliza **Clean Architecture** com camadas bem definidas:

- **`Entities`**: Modelos de domínio puros (ex: `Usuario`, `Avaliacao`)
- **`Use Cases/Core`**: Lógica de negócio nas `Services`
- **`Interface Adapters`**: `Controllers` e `DTOs`
- **`Infrastructure`**: `DbContext` e configurações do EF Core

## ⚙️ Princípios SOLID

| Princípio | Implementação |
|-----------|---------------|
| **Single Responsibility (SRP)** | - Cada Service tem uma única responsabilidade (ex: `UsuarioServices` só lida com usuários)<br>- Controllers apenas orquestram chamadas |
| **Open/Closed (OCP)** | - Entidades estendíveis sem modificar comportamento existente<br>- Novos recursos via composição (ex: avaliações não modificaram a entidade Jogo) |
| **Liskov Substitution (LSP)** | Uso consistente de interfaces (todos os DTOs seguem mesmo padrão) |
| **Interface Segregation (ISP)** | Múltiplos DTOs específicos (ex: `CriarAvaliacaoDTO` vs `AvaliacaoDTO`) |
| **Dependency Inversion (DIP)** | - Injeção de dependência em controllers e services<br>- Infraestrutura injetada via construtor |

## 🚀 Como Executar

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/pt-br/sql-server/sql-server-downloads) (ou Docker para container)
- Arquivo `.env` configurado

### 🔧 Configuração Inicial

1. **Clone esse repositório e acesse sua raiz**

```git clone https://github.com/GameLogger/GameLog-BackEnd.git```

```cd GameLog-BackEnd ```

3. **Crie o arquivo `.env`** na raiz do projeto com:

```env
DB_SERVER=seu_servidor
DB_NAME=GameLog
DB_USER=seu_usuario
DB_PASSWORD=sua_senha
JWT_SECRET=chave_secreta_32_chars
```
3. **Execute no seu terminal, na raiz do seu projeto, os seguintes comandos:**

```dotnet restore```

```dotnet run ```

O comando "dotnet restore" irá restaurar pacotes e dependências, e o comando "dotnet run" irá executar a aplicação 
Uma migration para modelar seu banco de dados será executada automaticamente, seguida de um seeder para popular as tabelas de Empresas, Jogos, Generos e JogoGenero (tabela associativa)

Também, em seu navegador, será aberto o *swagger* com todos os endpoints utilizados e seus respectivos payloads. Caso haja algum problema nessa etapa, tente acessar https://localhost:7096/swagger/index.html com a aplicação rodando).

Por fim, para ter a experiência completa, também siga o passo a passo para executar nosso frontend, acessível em: https://github.com/GameLogger/GameLog-FrontEnd .
