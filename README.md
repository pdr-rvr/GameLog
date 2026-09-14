# 🎮 GameLog — A Rede Social e Plataforma de Gestão Definitiva para Gamers

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React 19](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=black)](https://react.dev/)
[![PostgreSQL 16](https://img.shields.io/badge/PostgreSQL-16_Alpine-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker Compose](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![Testes Automatizados](https://img.shields.io/badge/Testes-233_Passando-success?logo=checkmarx&logoColor=white)](tests/)

**GameLog** é uma plataforma full-stack moderna inspirada em redes de catalogação cultural (como *Letterboxd* e *Goodreads*), projetada sob medida para entusiastas de jogos eletrônicos. 

O GameLog permite aos gamers explorar um catálogo com **mais de 6.300 títulos reais**, organizar sua biblioteca pessoal em categorias de progresso (*Jogando, Zerado, Quero Jogar, Pausado, Abandonado*), registrar avaliações e resenhas detalhadas com notas de 1 a 5 estrelas, criar coleções temáticas personalizadas, montar seu Top 5 de jogos favoritos da vida, interagir socialmente em discussões com amigos e receber recomendações inteligentes com base no seu perfil gamer.

---

## 🌟 Principais Funcionalidades

### 1. 📚 Biblioteca Pessoal & Gestão de Backlog
- **Organização por Status de Progresso:** Classifique qualquer jogo como:
  - 🎮 **Jogando** — Acompanhe o que você está experimentando no momento.
  - 🏆 **Zerado** — Registre suas conquistas e jogos finalizados.
  - ⏳ **Quero Jogar** — Monte sua fila de desejos e nunca esqueça o próximo título.
  - ⏸️ **Pausado** — Mantenha na mira jogos deixados para depois.
  - 🛑 **Abandonado** — Marque títulos que você decidiu não continuar.
- **Contadores Instantâneos & Filtros:** Visualize na hora o total de jogos em cada categoria e filtre sua biblioteca por status ou busca por nome.

### 2. ⭐ Avaliações, Resenhas & Sistema Comunitário
- **Notas e Resenhas Detalhadas:** Avalie de 1 a 5 estrelas com resenhas textuais de até 1.000 caracteres compartilhando sua opinião crítica.
- **Interação Social:** Curta avaliações de outros jogadores e participe de debates através de threads encadeadas de respostas.
- **Controle Estrito de Integridade:** Regras de autorização garantem que apenas o autor possa editar ou excluir sua própria avaliação, prevenindo race conditions e mantendo histórico íntegro.

### 3. 🥇 Pódio dos Favoritos: Top 5 Jogos da Vida
- **Destaque Visual Estilo Letterboxd:** Escolha e fixe os 5 jogos mais marcantes da sua jornada gamer no topo do seu perfil público.
- **Classificação Visual Especial:** Destaque de 1º a 5º lugar com estilização em Ouro, Prata e Bronze.

### 4. 📋 Coleções & Listas Temáticas Customizadas
- **Crie Listas Personalizadas:** Monte seleções temáticas como *"Melhores RPGs de Turno"*, *"Jogos que me Fizeram Chorar"*, *"Souls-like que Vale a Pena Jogar"*.
- **Mosaicos Dinâmicos:** Pré-visualização automática com mosaico estilizado das capas dos primeiros jogos adicionados à lista.
- **Visibilidade Flexível:** Defina coleções como públicas para a comunidade ou privadas para controle próprio.

### 5. 👥 Rede Social Gamer & Feed de Atividades
- **Siga Amigos e Criadores:** Conecte-se com outros perfis e descubra o que eles estão jogando e achando dos títulos.
- **Feed Social em Tempo Real:** Linha do tempo unificada com as atividades recentes dos perfis seguidos (novas reviews publicadas, jogos finalizados, títulos adicionados à biblioteca e novas coleções).
- **Sidebar Dinâmica:** Descubra amigos ativos e sugestões personalizadas de novos jogadores com gostos semelhantes para seguir.

### 6. 🧠 Motor de Recomendação Personalizada
- **Algoritmo de Afinidade Multi-Fator:** O GameLog analisa o histórico do jogador cruzando:
  - Gêneros com maiores notas dadas pelo usuário;
  - Desenvolvedoras e estúdios com maior presença na biblioteca do gamer;
  - Jogos em alta com melhor avaliação entre as pessoas que o usuário segue.
- **Aceleração por Cache:** Resultados calculados e cacheados em memória por 10 minutos por jogador, proporcionando carregamento instantâneo.

### 7. 🔍 Catálogo Rico & Busca Global Instantânea
- **+6.300 Títulos Canônicos:** Base rica com sinopses em português, capas verticais em alta definição, classificação indicativa (Livre a 18 anos) e metadados oficiais de estúdios e publicadoras.
- **Busca por Trigramas (pg_trgm):** Mecanismo de busca no banco com operador GIN acelerado (< 0.3ms), tolerante a variações de digitação e acentuação.
- **Filtros Avançados:** Filtre o catálogo por gênero, ano de lançamento, estúdio desenvolvedor, publicadora e nota média.
- **Busca Híbrida RAWG:** Integração transparente com a API RAWG para importar e exibir novos títulos sob demanda.

### 8. 👤 Perfil Gamer Completo
- Avatar personalizado, biografia e estatísticas da jornada gamer (total de jogos na biblioteca, jogos zerados, média de notas atribuídas e gêneros favoritos).
- Abas modulares dedicadas para navegar pela Biblioteca, Avaliações Publicadas e Coleções do jogador.

---

## 🏛️ Arquitetura e Engenharia do Sistema

O GameLog foi desenvolvido seguindo padrões rigorosos de engenharia de software, separação de responsabilidades (SOLID), resiliência e segurança defensiva:

```text
┌─────────────────────────────────────────────────────────────┐
│                    Navegador Web (Cliente)                  │
│          React 19 SPA (Porta 3000 / Nginx Alpine com CSP)   │
│       Access Token em Memória + Cookie HttpOnly (Refresh)   │
└──────────────┬───────────────────────────────▲──────────────┘
               │ HTTP / JSON (Axios + Silent Refresh)
               ▼                               │ Resposta REST (com X-Correlation-ID)
┌──────────────────────────────────────────────┴──────────────┐
│                  GameLog API (.NET 8 Web API)                │
│       Porta 7096 / CORS Restrito / Rate Limiting Nativo      │
├─────────────────────────────────────────────────────────────┤
│ Middleware Pipeline:                                        │
│   ├── CorrelationIdMiddleware (X-Correlation-ID + LogContext)│
│   ├── UseSerilogRequestLogging (JSON Estruturado)           │
│   ├── GlobalExceptionHandlerMiddleware (ProblemDetails RFC) │
│   ├── UseCors ("AllowReactApp")                             │
│   └── UseRateLimiter (AuthLimiter / ExternalApiLimiter)     │
├─────────────────────────────────────────────────────────────┤
│ Camada de Serviços Segregados:                              │
│   ├── IAuthService (Login, Refresh Tokens, BCrypt)          │
│   ├── IUserProfileService (Perfil, Avatar, Bio, Soft Delete)│
│   ├── ISocialService (Follow, Conexões, Contadores)         │
│   ├── IFeedService (Feed Unificado & Timeline)              │
│   ├── IAvaliacaoService (Regras de Domínio & Auth IDOR 403) │
│   ├── ICacheService / DistributedCacheService (IDistributed)│
│   └── IRawgApiService / SteamGridDb (Polly Circuit Breaker) │
├─────────────────────────────────────────────────────────────┤
│ Persistência: EF Core 8 + AuditSaveChangesInterceptor       │
└───────┬──────────────────────────────────────────────┬──────┘
        │ EF Core 8 (Npgsql - UUIDv7 + pg_trgm GIN)    │ HTTP Resiliente (Polly)
        ▼                                              ▼
┌──────────────────────────────┐              ┌───────────────────────┐
│     PostgreSQL 16 Alpine     │              │     RAWG / SteamGrid  │
│  (6.300+ Jogos, AuditLogs,   │              │      External APIs    │
│   RefreshTokens, Avaliações) │              │ (Timeout / CB 50% 30s)│
└──────────────────────────────┘              └───────────────────────┘
```

### Destaques Técnicos de Destaque
- **Identificadores UUIDv7 (RFC 9562):** Todas as entidades utilizam UUIDs sequenciais no tempo, combinando a segurança anti-enumeração com a eficiência máxima de indexação B-Tree no PostgreSQL.
- **Autenticação com Rotating Refresh Tokens:** Access Token curto (15 min) mantido **apenas em memória** no cliente SPA e Refresh Token rotativo persistido no banco via cookie seguro `HttpOnly`, `SameSite=Lax` com **detecção ativa de reuso** (*compromise detection*).
- **Trilha de Auditoria Imutável (EF Core Interceptor):** Mutações de dados (INSERT, UPDATE, DELETE) são interceptadas automaticamente pelo `AuditSaveChangesInterceptor`, gravando diffs JSON antes/depois na tabela `AuditLogs` com sanitização automática de segredos (`[REDACTED]`).
- **Observabilidade & Serilog:** Logging estruturado em formato Compact JSON em produção, com rastreabilidade ponta a ponta vinculando requisições ao cabeçalho `X-Correlation-ID` e aos envelopes de erro `ProblemDetails`.
- **Resiliência com Polly Circuit Breaker:** Chamadas externas contra RAWG e SteamGridDB contam com timeout de tentativa (5s), timeout total (10s) e Circuit Breaker de 30s (desarme com 50% de falhas).
- **Aceleração Textual com pg_trgm:** Consultas textuais de catálogo utilizam índice GIN (`gin_trgm_ops`) no PostgreSQL, com tempo de busca inferior a 0.3 milissegundos.
- **Cache Desacoplado:** Interface `ICacheService` e implementação `DistributedCacheService` pronta para escalabilidade horizontal com Redis sem alteração de domínio.

---

## 🛠️ Stack Tecnológica

| Camada | Tecnologias |
|---|---|
| **Frontend** | React 19, React Router Dom v7, Vite 6, Axios com interceptors de silent refresh, Context API, CSS3 Glassmorphism, React Icons |
| **Backend** | .NET 8, ASP.NET Core Web API, EF Core 8, Npgsql com `EnableRetryOnFailure`, Serilog, Polly (`Microsoft.Extensions.Http.Resilience`), FluentValidation, BCrypt.Net, AutoMapper |
| **Banco de Dados** | PostgreSQL 16 Alpine, extensão `pg_trgm`, índices GIN e UUIDv7 |
| **Infraestrutura** | Docker, Docker Compose, Nginx Alpine com Content Security Policy (CSP), Usuário não-root (`USER $APP_UID`) |
| **Testes** | xUnit, FluentAssertions, Moq, WebApplicationFactory, Vitest, Testing Library, Coverlet |

---

## 📁 Estrutura de Diretórios

```
GameLog/
├── backend/                                  # API REST em .NET 8 (C#)
│   ├── Configurations/                       # Mapeamentos relacionais do EF Core
│   ├── Controllers/                          # Controllers REST segregados
│   ├── Database/                             # DbContext (GameLogContext)
│   ├── DTOs/                                 # Data Transfer Objects com validação fluente
│   ├── Entities/                             # Modelos de Domínio com UUIDv7
│   ├── Helpers/                              # Utilitários (UuidV7Helper)
│   ├── Interceptors/                         # Interceptor de auditoria imutável (AuditSaveChangesInterceptor)
│   ├── Middlewares/                          # Middlewares (CorrelationId, GlobalExceptionHandler)
│   ├── Migrations/                           # Migrações canônicas do PostgreSQL
│   ├── Profiles/                             # Mapeamentos AutoMapper
│   ├── Seeders/                              # Catálogo de 6.300+ jogos e normalizadores canônicos
│   ├── Services/                             # Regras de Negócio e Cache Desacoplado
│   ├── Validators/                           # Regras de validação com FluentValidation
│   ├── Dockerfile                            # Imagem Docker otimizada com usuário não-root
│   └── Program.cs                            # Configuração, pipeline de observabilidade e DI
├── frontend/                                 # Aplicação SPA em React 19
│   ├── public/                               # Assets estáticos
│   ├── src/
│   │   ├── components/                       # Componentes modulares reutilizáveis
│   │   ├── context/                          # AuthContext memoizado
│   │   ├── pages/                            # Telas (Home, Jogos, Perfil, Feed, Comunidade, Listas)
│   │   ├── services/                         # Clientes Axios com silent refresh
│   │   └── utils/                            # Utilitários e logger configurável
│   ├── nginx.conf                            # Nginx com headers defensivos (CSP, HSTS)
│   └── Dockerfile                            # Imagem Docker de produção
├── tests/                                    # Suítes de Testes Automatizados (233 testes)
│   ├── GameLog.Tests.Unit/                   # Testes Unitários de Backend (161 testes)
│   └── GameLog.Tests.Integration/            # Testes de Integração de Backend (30 testes)
├── docker-compose.yml                        # Orquestração (PostgreSQL 16 + Backend + Frontend)
├── .env.example                              # Modelo documentado de variáveis de ambiente
├── .gitignore                                # Regras de exclusão Git unificadas
└── README.md                                 # Documentação oficial do projeto
```

---

## 🚀 Como Executar o Projeto

A maneira recomendada para iniciar toda a aplicação com banco de dados, backend e frontend sincronizados é utilizando o **Docker Compose**:

### 1. Pré-requisitos
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e em execução.

### 2. Configurar o Arquivo `.env`
Copie o modelo de ambiente na raiz do projeto:

```bash
cp .env.example .env
```

> [!IMPORTANT]
> A aplicação segue o princípio **Fail-Fast**: as variáveis `DB_PASSWORD` e `JWT_SECRET` (mínimo de 32 caracteres / 256 bits) são estritamente obrigatórias. Se não configuradas no `.env`, a API aborta a inicialização na hora para evitar execução insegura.

### 3. Iniciar com Docker Compose
Execute o comando de compilação e inicialização:

```bash
docker compose up --build -d
```

### 4. Acessar os Serviços
- 🌐 **Frontend (Aplicação Web):** [http://localhost:3000](http://localhost:3000)
- ⚙️ **Backend (Documentação Swagger):** [http://localhost:7096/swagger](http://localhost:7096/swagger)
- 🩺 **Health Check da API:** [http://localhost:7096/health](http://localhost:7096/health)
- 🗄️ **PostgreSQL 16:** `localhost:5432` (Database: `gamelog`, Usuário: `postgres`)

> 💡 **Nota de Inicialização:** Na primeira subida, o backend executará automaticamente as migrações canônicas do Entity Framework Core e o seeder carregará a base canônica com mais de 6.300 jogos reais.

### 🔑 Credenciais de Demonstração (Já Semeadas)
Para testar a plataforma imediatamente com dados reais:
- **Usuário:** `pedro@gamelog.com` | **Senha:** `Password123!`
- **Usuário:** `ana@gamelog.com` | **Senha:** `Password123!`

### 5. Parar os Serviços
```bash
docker compose down
```

---

## 🧪 Suíte de Testes Automatizados

O projeto possui **233 testes automatizados** (100% *green*) cobrindo testes unitários, testes de integração de API e testes de componentes frontend:

```bash
# 1. Testes Unitários do Backend (161 testes)
docker run --rm -v "${PWD}:/app" -w /app mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet test tests/GameLog.Tests.Unit/GameLog.Tests.Unit.csproj

# 2. Testes de Integração do Backend (30 testes)
docker run --rm -v "${PWD}:/app" -w /app mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet test tests/GameLog.Tests.Integration/GameLog.Tests.Integration.csproj

# 3. Testes do Frontend com Vitest (42 testes)
docker run --rm -v "${PWD}/frontend:/app" -w /app node:20-alpine npm test -- --run
```

---

## 📄 Licença

Este projeto é desenvolvido para fins de portfólio de engenharia e aprendizado de arquiteturas escaláveis em .NET e React.
