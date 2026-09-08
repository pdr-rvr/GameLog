# 🎮 GameLog

GameLog é uma plataforma completa de catálogo, avaliação e recomendação de jogos eletrônicos. O sistema permite pesquisar títulos, filtrar por gêneros e anos, registrar avaliações e obter sugestões personalizadas com base no gosto do usuário.

---

## 📁 Estrutura do Projeto

```
GameLog/
├── backend/                                  # API REST em .NET 8 (C#)
│   ├── Configurations/                       # Configurações do EF Core e JWT
│   ├── Controllers/                          # Endpoints REST (Jogos, Avaliações, Usuários)
│   ├── Database/                             # DbContext do EF Core
│   ├── DTOs/                                 # Data Transfer Objects
│   ├── Entities/                             # Modelos de Domínio
│   ├── Migrations/                           # Migrações do banco de dados
│   ├── Profiles/                             # Mapeamentos AutoMapper
│   ├── Seeders/                              # Seeders automáticos de jogos e gêneros
│   ├── Services/                             # Regras de Negócio e Serviços
│   ├── Dockerfile                            # Build e imagem do backend
│   └── Program.cs                            # Configuração e inicialização da API
├── frontend/                                 # Single Page Application em React
│   ├── public/                               # Assets estáticos e capas de jogos
│   ├── src/
│   │   ├── components/                       # Componentes reutilizáveis (Navbar, Cards, Carrosséis...)
│   │   ├── context/                          # Contexto de autenticação JWT (AuthContext)
│   │   ├── pages/                            # Telas da aplicação e actions
│   │   └── services/                         # Configuração centralizada do Axios e AuthService
│   ├── nginx.conf                            # Configuração do Nginx para SPA
│   └── Dockerfile                            # Build e imagem do frontend
├── docker-compose.yml                        # Orquestração (SQL Server + Backend + Frontend)
├── .env.example                              # Modelo de variáveis de ambiente
├── .gitignore                                # Regras unificadas de ignore
└── README.md                                 # Documentação do projeto
```

---

## 🛠️ Tecnologias Utilizadas

### Backend
- **.NET 8 (ASP.NET Core Web API)**
- **Entity Framework Core 8** (Code-First & Migrations)
- **SQL Server 2022**
- **BCrypt.Net-Next** (Hashing seguro de senhas com Salt)
- **JWT (JSON Web Token) Bearer Authentication**
- **AutoMapper**
- **Swagger / OpenAPI**

### Frontend
- **React 19**
- **React Router Dom v7**
- **Axios**
- **jwt-decode**
- **React Icons**
- **CSS3 com Design Responsivo**

### Infraestrutura & Containerização
- **Docker & Docker Compose**
- **Nginx Alpine** (Servidor estático para o React)
- **Microsoft SQL Server Linux Container**

---

## 🚀 Como Executar com Docker Compose (Recomendado)

A forma mais rápida e simples de subir todo o ambiente (Banco de dados, API e Frontend) é utilizando o Docker Compose:

### 1. Pré-requisitos
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e em execução.

### 2. Iniciar a Aplicação
Na raiz do projeto, execute o comando:

```bash
docker compose up --build -d
```

### 3. Acessar os Serviços
- 🌐 **Frontend (Aplicação Web):** [http://localhost:3000](http://localhost:3000)
- ⚙️ **Backend (Documentação Swagger):** [http://localhost:7096/swagger](http://localhost:7096/swagger)
- 🗄️ **SQL Server:** `localhost:1433` (Usuário: `sa` / Senha: `GameLog123!@#`)

> 💡 **Nota:** Na primeira inicialização, o backend aplicará automaticamente as Migrações do banco de dados e executará os Seeders com mais de 40 jogos e gêneros cadastrados.

### 4. Parar a Aplicação
Para encerrar os containers:
```bash
docker compose down
```

---

## 💻 Execução Manual / Desenvolvimento Local

Se desejar executar os serviços fora do Docker:

### 1. Backend (.NET 8)
1. Configure o banco de dados SQL Server no seu `.env` ou `appsettings.json`.
2. Acesse a pasta `backend` e execute:
   ```bash
   cd backend
   dotnet restore
   dotnet run
   ```

### 2. Frontend (React)
1. Acesse a pasta `frontend` e instale as dependências:
   ```bash
   cd frontend
   npm install --legacy-peer-deps
   npm start
   ```
2. Acesse [http://localhost:3000](http://localhost:3000).

---

## 🔒 Segurança e Boas Práticas Implementadas

- **Proteção de Senhas:** Senhas criptografadas com `BCrypt` com fator de trabalho 11.
- **Validação de Autorização (BOLA / IDOR):** Endpoints de usuário verificam a identidade do usuário autenticado no token JWT antes de permitir edições ou exclusões.
- **Soft-Delete Seguro:** Usuários e avaliações desativados são devidamente filtrados nas consultas e no fluxo de autenticação.
- **Clean JSON Response:** Sem poluição de metadados internos de referência ciclíca (`$values`), retornando arrays e objetos REST padronizados.
