CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "Empresa" (
        "EmpresaId" uuid NOT NULL,
        "NomeEmpresa" character varying(150) NOT NULL,
        "EstaAtivo" boolean NOT NULL,
        CONSTRAINT "PK_Empresa" PRIMARY KEY ("EmpresaId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "Generos" (
        "GeneroId" uuid NOT NULL,
        "TituloGenero" character varying(50) NOT NULL,
        "EstaAtivo" boolean NOT NULL,
        CONSTRAINT "PK_Generos" PRIMARY KEY ("GeneroId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "Usuarios" (
        "UsuarioId" uuid NOT NULL,
        "NomeUsuario" character varying(30) NOT NULL,
        "Email" character varying(100) NOT NULL,
        "Senha" character varying(128) NOT NULL,
        "FotoDePerfil" text,
        "Bio" character varying(300),
        "EstaAtivo" boolean NOT NULL,
        CONSTRAINT "PK_Usuarios" PRIMARY KEY ("UsuarioId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "Jogos" (
        "JogoId" uuid NOT NULL,
        "Titulo" character varying(250) NOT NULL,
        "Descricao" text,
        "Imagem" text,
        "DataLancamento" date NOT NULL,
        "ClassificacaoIndicativa" integer NOT NULL,
        "EmpresaId" uuid NOT NULL,
        "PublicadoraId" uuid,
        "EstaAtivo" boolean NOT NULL,
        CONSTRAINT "PK_Jogos" PRIMARY KEY ("JogoId"),
        CONSTRAINT "FK_Jogos_Empresa_EmpresaId" FOREIGN KEY ("EmpresaId") REFERENCES "Empresa" ("EmpresaId") ON DELETE RESTRICT,
        CONSTRAINT "FK_Jogos_Empresa_PublicadoraId" FOREIGN KEY ("PublicadoraId") REFERENCES "Empresa" ("EmpresaId") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "ListasDeJogos" (
        "ListaDeJogosId" uuid NOT NULL,
        "UsuarioId" uuid NOT NULL,
        "Titulo" character varying(100) NOT NULL,
        "Descricao" character varying(500),
        "EstaPublica" boolean NOT NULL DEFAULT TRUE,
        "DataCriacao" timestamp with time zone NOT NULL,
        "DataAtualizacao" timestamp with time zone NOT NULL,
        "EstaAtivo" boolean NOT NULL DEFAULT TRUE,
        CONSTRAINT "PK_ListasDeJogos" PRIMARY KEY ("ListaDeJogosId"),
        CONSTRAINT "FK_ListasDeJogos_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("UsuarioId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "RefreshTokens" (
        "RefreshTokenId" uuid NOT NULL,
        "Token" character varying(256) NOT NULL,
        "UsuarioId" uuid NOT NULL,
        "DataCriacao" timestamp with time zone NOT NULL,
        "DataExpiracao" timestamp with time zone NOT NULL,
        "RevogadoEm" timestamp with time zone,
        "CriadoPorIp" text,
        "RevogadoPorIp" text,
        "SubstituidoPorToken" text,
        "EstaAtivo" boolean NOT NULL,
        CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("RefreshTokenId"),
        CONSTRAINT "FK_RefreshTokens_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("UsuarioId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "SegueUsuarios" (
        "Id" uuid NOT NULL,
        "UsuarioSeguidorId" uuid NOT NULL,
        "UsuarioSeguidoId" uuid NOT NULL,
        "EstaAtivo" boolean NOT NULL,
        CONSTRAINT "PK_SegueUsuarios" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_SegueUsuarios_Usuarios_UsuarioSeguidoId" FOREIGN KEY ("UsuarioSeguidoId") REFERENCES "Usuarios" ("UsuarioId") ON DELETE RESTRICT,
        CONSTRAINT "FK_SegueUsuarios_Usuarios_UsuarioSeguidorId" FOREIGN KEY ("UsuarioSeguidorId") REFERENCES "Usuarios" ("UsuarioId") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "Avaliacoes" (
        "AvaliacaoId" uuid NOT NULL,
        "Nota" integer NOT NULL,
        "JogoId" uuid NOT NULL,
        "UsuarioId" uuid NOT NULL,
        "TextoAvaliacao" character varying(500) NOT NULL,
        "DataPublicacao" timestamp with time zone NOT NULL,
        "EstaAtivo" boolean NOT NULL,
        CONSTRAINT "PK_Avaliacoes" PRIMARY KEY ("AvaliacaoId"),
        CONSTRAINT "CK_Avaliacao_Nota_Range" CHECK ("Nota" >= 0 AND "Nota" <= 5),
        CONSTRAINT "FK_Avaliacoes_Jogos_JogoId" FOREIGN KEY ("JogoId") REFERENCES "Jogos" ("JogoId") ON DELETE CASCADE,
        CONSTRAINT "FK_Avaliacoes_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("UsuarioId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "ItensBiblioteca" (
        "BibliotecaJogoId" uuid NOT NULL,
        "UsuarioId" uuid NOT NULL,
        "JogoId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "DataAtualizacao" timestamp with time zone NOT NULL,
        "DataConclusao" timestamp with time zone,
        "EstaAtivo" boolean NOT NULL DEFAULT TRUE,
        CONSTRAINT "PK_ItensBiblioteca" PRIMARY KEY ("BibliotecaJogoId"),
        CONSTRAINT "FK_ItensBiblioteca_Jogos_JogoId" FOREIGN KEY ("JogoId") REFERENCES "Jogos" ("JogoId") ON DELETE CASCADE,
        CONSTRAINT "FK_ItensBiblioteca_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("UsuarioId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "JogoGenero" (
        "GenerosId" uuid NOT NULL,
        "JogosId" uuid NOT NULL,
        CONSTRAINT "PK_JogoGenero" PRIMARY KEY ("GenerosId", "JogosId"),
        CONSTRAINT "FK_JogoGenero_Generos_GenerosId" FOREIGN KEY ("GenerosId") REFERENCES "Generos" ("GeneroId") ON DELETE CASCADE,
        CONSTRAINT "FK_JogoGenero_Jogos_JogosId" FOREIGN KEY ("JogosId") REFERENCES "Jogos" ("JogoId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "JogosFavoritosUsuarios" (
        "JogoFavoritoUsuarioId" uuid NOT NULL,
        "UsuarioId" uuid NOT NULL,
        "JogoId" uuid NOT NULL,
        "Posicao" integer NOT NULL,
        "EstaAtivo" boolean NOT NULL DEFAULT TRUE,
        CONSTRAINT "PK_JogosFavoritosUsuarios" PRIMARY KEY ("JogoFavoritoUsuarioId"),
        CONSTRAINT "FK_JogosFavoritosUsuarios_Jogos_JogoId" FOREIGN KEY ("JogoId") REFERENCES "Jogos" ("JogoId") ON DELETE CASCADE,
        CONSTRAINT "FK_JogosFavoritosUsuarios_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("UsuarioId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "ItensDeListas" (
        "ItemDeListaId" uuid NOT NULL,
        "ListaDeJogosId" uuid NOT NULL,
        "JogoId" uuid NOT NULL,
        "Ordem" integer NOT NULL,
        "DataAdicionado" timestamp with time zone NOT NULL,
        "EstaAtivo" boolean NOT NULL DEFAULT TRUE,
        CONSTRAINT "PK_ItensDeListas" PRIMARY KEY ("ItemDeListaId"),
        CONSTRAINT "FK_ItensDeListas_Jogos_JogoId" FOREIGN KEY ("JogoId") REFERENCES "Jogos" ("JogoId") ON DELETE CASCADE,
        CONSTRAINT "FK_ItensDeListas_ListasDeJogos_ListaDeJogosId" FOREIGN KEY ("ListaDeJogosId") REFERENCES "ListasDeJogos" ("ListaDeJogosId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "CurtidasDeAvaliacoes" (
        "CurtidaDeAvaliacaoId" uuid NOT NULL,
        "Curtida" boolean NOT NULL,
        "AvaliacaoId" uuid,
        "UsuarioId" uuid,
        "EstaAtivo" boolean NOT NULL,
        CONSTRAINT "PK_CurtidasDeAvaliacoes" PRIMARY KEY ("CurtidaDeAvaliacaoId"),
        CONSTRAINT "FK_CurtidasDeAvaliacoes_Avaliacoes_AvaliacaoId" FOREIGN KEY ("AvaliacaoId") REFERENCES "Avaliacoes" ("AvaliacaoId") ON DELETE CASCADE,
        CONSTRAINT "FK_CurtidasDeAvaliacoes_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("UsuarioId") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "RespostasDeAvaliacao" (
        "RespostaDeAvaliacaoId" uuid NOT NULL,
        "Comentario" character varying(500) NOT NULL,
        "AvaliacaoId" uuid,
        "UsuarioId" uuid,
        "DataCriacao" timestamp with time zone NOT NULL,
        "EstaAtivo" boolean NOT NULL,
        CONSTRAINT "PK_RespostasDeAvaliacao" PRIMARY KEY ("RespostaDeAvaliacaoId"),
        CONSTRAINT "FK_RespostasDeAvaliacao_Avaliacoes_AvaliacaoId" FOREIGN KEY ("AvaliacaoId") REFERENCES "Avaliacoes" ("AvaliacaoId") ON DELETE CASCADE,
        CONSTRAINT "FK_RespostasDeAvaliacao_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("UsuarioId") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE TABLE "CurtidasDeRespostas" (
        "CurtidaDeRespostaId" uuid NOT NULL,
        "Curtida" boolean NOT NULL,
        "RespostaDeAvaliacaoId" uuid,
        "UsuarioId" uuid,
        "EstaAtivo" boolean NOT NULL,
        CONSTRAINT "PK_CurtidasDeRespostas" PRIMARY KEY ("CurtidaDeRespostaId"),
        CONSTRAINT "FK_CurtidasDeRespostas_RespostasDeAvaliacao_RespostaDeAvaliaca~" FOREIGN KEY ("RespostaDeAvaliacaoId") REFERENCES "RespostasDeAvaliacao" ("RespostaDeAvaliacaoId") ON DELETE CASCADE,
        CONSTRAINT "FK_CurtidasDeRespostas_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("UsuarioId") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_Avaliacoes_DataPublicacao" ON "Avaliacoes" ("DataPublicacao");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_Avaliacoes_EstaAtivo" ON "Avaliacoes" ("EstaAtivo");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_Avaliacoes_JogoId" ON "Avaliacoes" ("JogoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Avaliacoes_UsuarioId_JogoId" ON "Avaliacoes" ("UsuarioId", "JogoId") WHERE "EstaAtivo" = true;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_CurtidasDeAvaliacoes_AvaliacaoId" ON "CurtidasDeAvaliacoes" ("AvaliacaoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_CurtidasDeAvaliacoes_UsuarioId" ON "CurtidasDeAvaliacoes" ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_CurtidasDeRespostas_RespostaDeAvaliacaoId" ON "CurtidasDeRespostas" ("RespostaDeAvaliacaoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_CurtidasDeRespostas_UsuarioId" ON "CurtidasDeRespostas" ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Empresa_NomeEmpresa" ON "Empresa" ("NomeEmpresa");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Generos_TituloGenero" ON "Generos" ("TituloGenero");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_ItensBiblioteca_DataAtualizacao" ON "ItensBiblioteca" ("DataAtualizacao");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_ItensBiblioteca_EstaAtivo" ON "ItensBiblioteca" ("EstaAtivo");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_ItensBiblioteca_JogoId" ON "ItensBiblioteca" ("JogoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_ItensBiblioteca_UsuarioId_JogoId" ON "ItensBiblioteca" ("UsuarioId", "JogoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_ItensDeListas_JogoId" ON "ItensDeListas" ("JogoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_ItensDeListas_ListaDeJogosId_JogoId" ON "ItensDeListas" ("ListaDeJogosId", "JogoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_JogoGenero_JogosId" ON "JogoGenero" ("JogosId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_Jogos_DataLancamento" ON "Jogos" ("DataLancamento");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_Jogos_EmpresaId" ON "Jogos" ("EmpresaId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_Jogos_EstaAtivo" ON "Jogos" ("EstaAtivo");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_Jogos_EstaAtivo_DataLancamento" ON "Jogos" ("EstaAtivo", "DataLancamento");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_Jogos_PublicadoraId" ON "Jogos" ("PublicadoraId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_Jogos_Titulo" ON "Jogos" ("Titulo");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_JogosFavoritosUsuarios_JogoId" ON "JogosFavoritosUsuarios" ("JogoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_JogosFavoritosUsuarios_UsuarioId_Posicao" ON "JogosFavoritosUsuarios" ("UsuarioId", "Posicao");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_ListasDeJogos_UsuarioId" ON "ListasDeJogos" ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_RefreshTokens_Token" ON "RefreshTokens" ("Token");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_RefreshTokens_UsuarioId" ON "RefreshTokens" ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_RespostasDeAvaliacao_AvaliacaoId" ON "RespostasDeAvaliacao" ("AvaliacaoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_RespostasDeAvaliacao_UsuarioId" ON "RespostasDeAvaliacao" ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_SegueUsuarios_UsuarioSeguidoId" ON "SegueUsuarios" ("UsuarioSeguidoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE INDEX "IX_SegueUsuarios_UsuarioSeguidorId" ON "SegueUsuarios" ("UsuarioSeguidorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Usuarios_Email" ON "Usuarios" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Usuarios_NomeUsuario" ON "Usuarios" ("NomeUsuario");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913195832_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260913195832_InitialCreate', '8.0.13');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913231150_AddTrigramIndex') THEN
    DROP INDEX IF EXISTS "IX_Jogos_Titulo";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913231150_AddTrigramIndex') THEN
    CREATE EXTENSION IF NOT EXISTS pg_trgm;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913231150_AddTrigramIndex') THEN
    CREATE INDEX idx_jogos_titulo_trgm ON "Jogos" USING gin ("Titulo" gin_trgm_ops);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913231150_AddTrigramIndex') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260913231150_AddTrigramIndex', '8.0.13');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913234431_AddAuditLogs') THEN
    CREATE TABLE "AuditLogs" (
        "AuditLogId" uuid NOT NULL,
        "UsuarioId" character varying(100),
        "Entidade" character varying(100) NOT NULL,
        "EntidadeId" character varying(100) NOT NULL,
        "TipoAcao" character varying(20) NOT NULL,
        "ValoresAntigos" text,
        "ValoresNovos" text,
        "TimestampUtc" timestamp with time zone NOT NULL,
        "IpAddress" character varying(50),
        "CorrelationId" character varying(100),
        CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("AuditLogId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913234431_AddAuditLogs') THEN
    CREATE INDEX "IX_AuditLogs_Entidade_EntidadeId" ON "AuditLogs" ("Entidade", "EntidadeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913234431_AddAuditLogs') THEN
    CREATE INDEX "IX_AuditLogs_TimestampUtc" ON "AuditLogs" ("TimestampUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913234431_AddAuditLogs') THEN
    CREATE INDEX "IX_AuditLogs_UsuarioId" ON "AuditLogs" ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260913234431_AddAuditLogs') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260913234431_AddAuditLogs', '8.0.13');
    END IF;
END $EF$;
COMMIT;

