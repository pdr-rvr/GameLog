# 🎮 GameLog

O **GameLog** é uma plataforma completa de catálogo, avaliação e recomendação personalizada de jogos, desenvolvida como projeto da disciplina de **Engenharia de Software** da turma **CP700TIN3** na instituição **FACENS**.

- **Backend**: API RESTful robusta desenvolvida em **.NET 8** com **Clean Architecture**, **Entity Framework Core**, **SQL Server** e autenticação **JWT**.
- **Frontend**: Aplicação web SPA interativa desenvolvida em **React** estruturada em camadas modulares inspiradas na **Clean Architecture**.

---

## 📁 Estrutura do Projeto

```
GameLog/
├── .github/
│   └── workflows/
│       └── azure-static-web-apps-happy-bay-0ed0e851e.yml  # CI/CD Azure Static Web Apps
├── backend/                                               # Código-fonte da API (.NET 8)
│   ├── .config/
│   ├── Configurations/                                    # Configurações do EF Core e injeções
│   ├── Controllers/                                       # Endpoints REST (Avaliações, Jogos, Usuários...)
│   ├── Database/                                          # DbContext do EF Core
│   ├── DTOs/                                              # Data Transfer Objects
│   ├── Entities/                                          # Modelos de Domínio
│   ├── Migrations/                                        # Migrações do banco de dados
│   ├── Profiles/                                          # Mapeamentos AutoMapper
│   ├── Seeders/                                           # Seeders para popular catálogo e gêneros
│   ├── Services/                                          # Regras de Negócio e Serviços
│   ├── GameLog_Backend.csproj
│   ├── GameLog_Backend.sln
│   └── Program.cs
├── frontend/                                              # Código-fonte da Aplicação Web (React)
│   ├── public/                                            # Assets estáticos e imagens dos jogos
│   ├── src/
│   │   ├── components/                                    # Componentes reutilizáveis (Navbar, Cards, Carrosséis...)
│   │   ├── context/                                       # Contextos de autenticação (AuthContext)
│   │   ├── pages/                                         # Telas da aplicação e suas respectivas actions
│   │   └── services/                                      # Clientes HTTP (Axios) e AuthService
│   ├── package.json
│   └── package-lock.json
├── .gitignore                                             # Regras unificadas de ignore (.NET, Node, React, IDEs)
└── README.md                                              # Documentação do projeto
```

---

## 🏗️ Arquitetura do Sistema

### 1. ⚙️ Backend (.NET 8 & Clean Architecture)
O backend é estruturado em camadas bem definidas e desacopladas:
- **`Entities`**: Modelos de domínio puros sem dependências externas (`Usuario`, `Jogo`, `Avaliacao`, `Genero`, `Empresa`).
- **`Use Cases / Services`**: Regras de negócio, cálculos de recomendação baseados em preferências de gênero e avaliações.
- **`Interface Adapters`**: `Controllers` REST e `DTOs` com validações.
- **`Infrastructure`**: `GameLogDbContext`, mapeamentos do Entity Framework Core, migrations e seeders automáticos.

#### Princípios SOLID no Backend
| Princípio | Implementação no Backend |
|---|---|
| **Single Responsibility (SRP)** | Cada Service tem uma única responsabilidade (ex: `UsuarioServices` lida apenas com usuários; `AvaliacaoServices` lida com avaliações). Controllers apenas orquestram chamadas. |
| **Open/Closed (OCP)** | Entidades e regras de recomendação estendíveis sem modificar o comportamento existente (ex: inclusão de avaliações sem mutação estrutural da entidade `Jogo`). |
| **Liskov Substitution (LSP)** | Uso consistente de interfaces e contratos homogêneos em todos os DTOs e entidades base (`Entity`, `IAuditable`). |
| **Interface Segregation (ISP)** | DTOs granulares e específicos para cada operação (ex: `CriarAvaliacaoDTO` vs `AvaliacaoDTO`). |
| **Dependency Inversion (DIP)** | Injeção de dependência nativa do ASP.NET Core em todos os controllers e serviços; persistência abstraída via `DbContext`. |

---

### 2. 🎨 Frontend (React & Modular Architecture)
O frontend segue uma estrutura modular inspirada na Clean Architecture adaptada para React:
- **`Presentation Layer` (`components/`, `pages/`)**: Componentes visuais desacoplados e páginas (`TelaHome`, `TelaLogin`, `TelaCadastro`, `PaginaJogos`, `MinhasAvaliacoes`, `PerfilUsuario`).
- **`Application Layer` (`pages/[Tela]/actions/`)**: Funções desacopladas para orquestração de chamadas à API e manipulação de estado (`cadastrarUsuario`, `buscarAvaliacoes`, etc.).
- **`Infrastructure Layer` (`services/`)**: Instância configurada do Axios (`api.js`) e gerenciador de autenticação JWT (`authService.js`).
- **`Auth & Sessão`**: Autenticação via JWT com armazenamento em `localStorage` e rotas protegidas (`ProtectedRoute`).

#### Princípios SOLID no Frontend
| Princípio | Implementação no Frontend |
|---|---|
| **Single Responsibility (SRP)** | Páginas e componentes possuem responsabilidades isoladas (ex: `FormAvaliacao` gerencia apenas a UI do formulário; a lógica de submissão está nas `actions`). |
| **Open/Closed (OCP)** | Componentes como `Navbar`, `AvaliacaoCard` e `JogosCarrossel` são reutilizáveis e extensíveis via `props`. |
| **Liskov Substitution (LSP)** | Componentes seguem contratos estritos de `props`, garantindo comportamento previsível em qualquer contexto de uso. |
| **Interface Segregation (ISP)** | Props bem delimitadas e específicas, evitando acoplamento desnecessário de dados. |
| **Dependency Inversion (DIP)** | Camada visual depende de abstrações de ações e serviços (`actions` e `services`), isolando a interface da implementação HTTP. |

---

## 🛠️ Tecnologias Utilizadas

### Backend
- **C# / .NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core 8** (Code-First, Migrations)
- **SQL Server**
- **AutoMapper**
- **JWT (JSON Web Token) Bearer Authentication**
- **Swagger / OpenAPI**
- **BCrypt.Net** (Hashing de senhas)

### Frontend
- **React 18**
- **React Router Dom**
- **Axios**
- **jwt-decode**
- **CSS Modular**
- **FontAwesome Icons**

---

## 🚀 Como Executar o Projeto

### 📋 Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (versão 18.x ou superior) & `npm`
- [SQL Server](https://www.microsoft.com/pt-br/sql-server/sql-server-downloads) (ou SQL Server via Docker)
- [Git](https://git-scm.com/)

---

### 1️⃣ Executando o Backend

1. **Acesse o diretório do backend:**
   ```bash
   cd backend
   ```

2. **Configure o arquivo `.env`:**
   Crie um arquivo `.env` dentro da pasta `backend/` com as credenciais do seu banco de dados e segredo JWT:
   ```env
   DB_SERVER=localhost
   DB_NAME=GameLog
   DB_USER=seu_usuario
   DB_PASSWORD=sua_senha
   JWT_SECRET=sua_chave_secreta_super_segura_com_no_minimo_32_caracteres
   ```

3. **Restaure as dependências e inicie a API:**
   ```bash
   dotnet restore
   dotnet run
   ```
   > 💡 As migrations do banco de dados e os seeders de jogos, gêneros e imagens serão executados automaticamente na inicialização.

4. **Acesse a documentação Swagger:**
   - Swagger UI: [https://localhost:7096/swagger](https://localhost:7096/swagger) ou [http://localhost:5242/swagger](http://localhost:5242/swagger)

---

### 2️⃣ Executando o Frontend

1. **Acesse o diretório do frontend:**
   ```bash
   cd frontend
   ```

2. **Instale as dependências:**
   ```bash
   npm install
   ```

3. **(Opcional) Configure a URL da API:**
   Caso necessário, configure um arquivo `.env` em `frontend/`:
   ```env
   REACT_APP_API_BASE_URL=https://localhost:7096
   ```

4. **Inicie a aplicação React:**
   ```bash
   npm start
   ```
   - O frontend será aberto automaticamente em: [http://localhost:3000](http://localhost:3000)

---

## 👥 Autores

Projeto desenvolvido pelos estudantes da disciplina de **Engenharia de Software (CP700TIN3)** na **FACENS**.
