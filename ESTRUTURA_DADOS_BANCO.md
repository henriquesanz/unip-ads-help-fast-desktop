# Estrutura de Dados e Banco de Dados - HelpFast

## 📋 **Visão Geral**

Este documento define todas as estruturas de dados, tabelas, relacionamentos e organização do banco de dados Azure SQL Database do sistema HelpFast.

## 🗄️ **Banco de Dados**

### **Especificações Técnicas**
- **SGBD:** Microsoft SQL Server (Azure SQL Database)
- **Tier:** Free Tier
- **Região:** Brasil Sul
- **Backup:** Automático (7 dias de retenção)
- **Criptografia:** Transparent Data Encryption (TDE) ativada

## 📊 **Estrutura das Tabelas**

### **1. Tabela: Usuarios**

**Descrição:** Armazena informações de todos os usuários do sistema (Clientes, Técnicos e Administradores).

```sql
CREATE TABLE Usuarios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nome NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Telefone NVARCHAR(15) NOT NULL,
    Senha NVARCHAR(255) NOT NULL, -- Hash BCrypt
    TipoUsuario INT NOT NULL, -- 1=Cliente, 2=Técnico, 3=Admin
    DataCriacao DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UltimoLogin DATETIME2 NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    CriadoPorId INT NULL,
    DataAtualizacao DATETIME2 NULL,
    CONSTRAINT FK_Usuarios_CriadoPor FOREIGN KEY (CriadoPorId) REFERENCES Usuarios(Id),
    CONSTRAINT CK_TipoUsuario CHECK (TipoUsuario IN (1, 2, 3))
);

-- Índices
CREATE INDEX IX_Usuarios_Email ON Usuarios(Email);
CREATE INDEX IX_Usuarios_TipoUsuario ON Usuarios(TipoUsuario);
CREATE INDEX IX_Usuarios_Ativo ON Usuarios(Ativo);
```

### **2. Tabela: Chamados**

**Descrição:** Armazena todos os chamados de suporte do sistema.

```sql
CREATE TABLE Chamados (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Titulo NVARCHAR(200) NOT NULL,
    Descricao NVARCHAR(2000) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Aberto',
    Prioridade NVARCHAR(50) NOT NULL DEFAULT 'Media',
    Categoria NVARCHAR(100) NULL, -- Categoria automática da IA
    Subcategoria NVARCHAR(100) NULL,
    DataCriacao DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DataAtualizacao DATETIME2 NULL,
    DataResolucao DATETIME2 NULL,
    DataFechamento DATETIME2 NULL,
    UsuarioId INT NOT NULL,
    TecnicoId INT NULL,
    TempoResolucao INT NULL, -- Em minutos
    Satisfacao INT NULL, -- 1-5 estrelas
    ComentarioSatisfacao NVARCHAR(500) NULL,
    CONSTRAINT FK_Chamados_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT FK_Chamados_Tecnico FOREIGN KEY (TecnicoId) REFERENCES Usuarios(Id),
    CONSTRAINT CK_Status CHECK (Status IN ('Aberto', 'Em Andamento', 'Resolvido', 'Fechado', 'Cancelado')),
    CONSTRAINT CK_Prioridade CHECK (Prioridade IN ('Baixa', 'Media', 'Alta', 'Critica')),
    CONSTRAINT CK_Satisfacao CHECK (Satisfacao BETWEEN 1 AND 5)
);

-- Índices
CREATE INDEX IX_Chamados_UsuarioId ON Chamados(UsuarioId);
CREATE INDEX IX_Chamados_TecnicoId ON Chamados(TecnicoId);
CREATE INDEX IX_Chamados_Status ON Chamados(Status);
CREATE INDEX IX_Chamados_Prioridade ON Chamados(Prioridade);
CREATE INDEX IX_Chamados_DataCriacao ON Chamados(DataCriacao);
CREATE INDEX IX_Chamados_Categoria ON Chamados(Categoria);
```

### **3. Tabela: Comentarios**

**Descrição:** Armazena comentários e histórico de conversas dos chamados.

```sql
CREATE TABLE Comentarios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ChamadoId INT NOT NULL,
    UsuarioId INT NOT NULL,
    Comentario NVARCHAR(2000) NOT NULL,
    Tipo NVARCHAR(50) NOT NULL DEFAULT 'Publico', -- Publico, Interno, Sistema
    DataCriacao DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    VisivelCliente BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Comentarios_Chamado FOREIGN KEY (ChamadoId) REFERENCES Chamados(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Comentarios_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT CK_TipoComentario CHECK (Tipo IN ('Publico', 'Interno', 'Sistema'))
);

-- Índices
CREATE INDEX IX_Comentarios_ChamadoId ON Comentarios(ChamadoId);
CREATE INDEX IX_Comentarios_UsuarioId ON Comentarios(UsuarioId);
CREATE INDEX IX_Comentarios_DataCriacao ON Comentarios(DataCriacao);
```

### **4. Tabela: Notificacoes**

**Descrição:** Armazena notificações do sistema para usuários.

```sql
CREATE TABLE Notificacoes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UsuarioId INT NOT NULL,
    Tipo NVARCHAR(50) NOT NULL,
    Titulo NVARCHAR(200) NOT NULL,
    Mensagem NVARCHAR(1000) NOT NULL,
    Prioridade NVARCHAR(20) NOT NULL DEFAULT 'Media',
    Lida BIT NOT NULL DEFAULT 0,
    DataEnvio DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DataLeitura DATETIME2 NULL,
    Canal NVARCHAR(50) NOT NULL DEFAULT 'InApp', -- InApp, Email, Push, SMS
    ChamadoId INT NULL,
    Acao NVARCHAR(100) NULL,
    CONSTRAINT FK_Notificacoes_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT FK_Notificacoes_Chamado FOREIGN KEY (ChamadoId) REFERENCES Chamados(Id),
    CONSTRAINT CK_PrioridadeNotificacao CHECK (Prioridade IN ('Baixa', 'Media', 'Alta', 'Urgente')),
    CONSTRAINT CK_CanalNotificacao CHECK (Canal IN ('InApp', 'Email', 'Push', 'SMS'))
);

-- Índices
CREATE INDEX IX_Notificacoes_UsuarioId ON Notificacoes(UsuarioId);
CREATE INDEX IX_Notificacoes_Lida ON Notificacoes(Lida);
CREATE INDEX IX_Notificacoes_DataEnvio ON Notificacoes(DataEnvio);
CREATE INDEX IX_Notificacoes_Tipo ON Notificacoes(Tipo);
```

### **5. Tabela: FAQ**

**Descrição:** Armazena perguntas frequentes e suas soluções.

```sql
CREATE TABLE FAQ (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Titulo NVARCHAR(200) NOT NULL,
    Descricao NVARCHAR(500) NOT NULL,
    Solucao NVARCHAR(2000) NOT NULL,
    Categoria NVARCHAR(100) NOT NULL,
    Subcategoria NVARCHAR(100) NULL,
    Tags NVARCHAR(500) NULL, -- Separadas por vírgula
    Visualizacoes INT NOT NULL DEFAULT 0,
    Utilidade INT NULL, -- Rating 1-5
    Ativo BIT NOT NULL DEFAULT 1,
    DataCriacao DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DataAtualizacao DATETIME2 NULL,
    CriadoPorId INT NOT NULL,
    CONSTRAINT FK_FAQ_CriadoPor FOREIGN KEY (CriadoPorId) REFERENCES Usuarios(Id),
    CONSTRAINT CK_UtilidadeFAQ CHECK (Utilidade BETWEEN 1 AND 5)
);

-- Índices
CREATE INDEX IX_FAQ_Categoria ON FAQ(Categoria);
CREATE INDEX IX_FAQ_Ativo ON FAQ(Ativo);
CREATE INDEX IX_FAQ_Visualizacoes ON FAQ(Visualizacoes DESC);
CREATE INDEX IX_FAQ_Tags ON FAQ(Tags);
```

### **6. Tabela: InteracoesIA**

**Descrição:** Armazena interações com a IA para análise e aprendizado.

```sql
CREATE TABLE InteracoesIA (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UsuarioId INT NOT NULL,
    TipoInteracao NVARCHAR(50) NOT NULL, -- Chat, Categorizacao, Atribuicao
    Pergunta NVARCHAR(1000) NULL,
    Resposta NVARCHAR(2000) NULL,
    Categoria NVARCHAR(100) NULL,
    ProblemaResolvido BIT NULL,
    Satisfacao INT NULL, -- 1-5
    TempoResposta INT NULL, -- Em segundos
    Confianca DECIMAL(3,2) NULL, -- 0.00 a 1.00
    DataInteracao DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ChamadoId INT NULL,
    CONSTRAINT FK_InteracoesIA_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT FK_InteracoesIA_Chamado FOREIGN KEY (ChamadoId) REFERENCES Chamados(Id),
    CONSTRAINT CK_TipoInteracao CHECK (TipoInteracao IN ('Chat', 'Categorizacao', 'Atribuicao')),
    CONSTRAINT CK_SatisfacaoIA CHECK (Satisfacao BETWEEN 1 AND 5),
    CONSTRAINT CK_ConfiancaIA CHECK (Confianca BETWEEN 0.00 AND 1.00)
);

-- Índices
CREATE INDEX IX_InteracoesIA_UsuarioId ON InteracoesIA(UsuarioId);
CREATE INDEX IX_InteracoesIA_TipoInteracao ON InteracoesIA(TipoInteracao);
CREATE INDEX IX_InteracoesIA_DataInteracao ON InteracoesIA(DataInteracao);
CREATE INDEX IX_InteracoesIA_ChamadoId ON InteracoesIA(ChamadoId);
```

### **7. Tabela: LogsAuditoria**

**Descrição:** Armazena logs de auditoria para conformidade LGPD e rastreabilidade.

```sql
CREATE TABLE LogsAuditoria (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UsuarioId INT NULL,
    Acao NVARCHAR(100) NOT NULL,
    Tabela NVARCHAR(50) NOT NULL,
    RegistroId INT NULL,
    DadosAntigos NVARCHAR(MAX) NULL, -- JSON
    DadosNovos NVARCHAR(MAX) NULL, -- JSON
    IPAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    DataAcao DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_LogsAuditoria_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);

-- Índices
CREATE INDEX IX_LogsAuditoria_UsuarioId ON LogsAuditoria(UsuarioId);
CREATE INDEX IX_LogsAuditoria_Acao ON LogsAuditoria(Acao);
CREATE INDEX IX_LogsAuditoria_Tabela ON LogsAuditoria(Tabela);
CREATE INDEX IX_LogsAuditoria_DataAcao ON LogsAuditoria(DataAcao);
```

### **8. Tabela: ConfiguracoesSistema**

**Descrição:** Armazena configurações do sistema.

```sql
CREATE TABLE ConfiguracoesSistema (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Chave NVARCHAR(100) UNIQUE NOT NULL,
    Valor NVARCHAR(500) NOT NULL,
    Tipo NVARCHAR(50) NOT NULL DEFAULT 'String', -- String, Int, Boolean, JSON
    Descricao NVARCHAR(200) NULL,
    DataAtualizacao DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AtualizadoPorId INT NOT NULL,
    CONSTRAINT FK_ConfiguracoesSistema_Usuario FOREIGN KEY (AtualizadoPorId) REFERENCES Usuarios(Id),
    CONSTRAINT CK_TipoConfiguracao CHECK (Tipo IN ('String', 'Int', 'Boolean', 'JSON'))
);

-- Índices
CREATE INDEX IX_ConfiguracoesSistema_Chave ON ConfiguracoesSistema(Chave);
```

### **9. Tabela: Metricas**

**Descrição:** Armazena métricas e KPIs do sistema para relatórios e dashboards.

```sql
CREATE TABLE Metricas (
    Id INT PRIMARY KEY IDENTITY(1,1),
    DataMetrica DATE NOT NULL,
    HoraMetrica TIME NOT NULL,
    TipoMetrica NVARCHAR(50) NOT NULL, -- Diaria, Semanal, Mensal
    ChamadosAbertos INT NOT NULL DEFAULT 0,
    ChamadosResolvidos INT NOT NULL DEFAULT 0,
    ChamadosFechados INT NOT NULL DEFAULT 0,
    ChamadosEscalados INT NOT NULL DEFAULT 0,
    TempoMedioResolucao DECIMAL(10,2) NULL, -- Em minutos
    SatisfacaoMedia DECIMAL(3,2) NULL, -- 1.00 a 5.00
    UsuariosAtivos INT NOT NULL DEFAULT 0,
    TecnicosAtivos INT NOT NULL DEFAULT 0,
    TaxaResolucaoPrimeiroContato DECIMAL(5,2) NULL, -- Percentual
    ChamadosPorPrioridade NVARCHAR(MAX) NULL, -- JSON
    ChamadosPorCategoria NVARCHAR(MAX) NULL, -- JSON
    TempoResolucaoPorPrioridade NVARCHAR(MAX) NULL, -- JSON
    DataCriacao DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_TipoMetrica CHECK (TipoMetrica IN ('Diaria', 'Semanal', 'Mensal')),
    CONSTRAINT CK_SatisfacaoMedia CHECK (SatisfacaoMedia BETWEEN 1.00 AND 5.00),
    CONSTRAINT CK_TaxaResolucao CHECK (TaxaResolucaoPrimeiroContato BETWEEN 0.00 AND 100.00)
);

-- Índices
CREATE INDEX IX_Metricas_DataMetrica ON Metricas(DataMetrica);
CREATE INDEX IX_Metricas_TipoMetrica ON Metricas(TipoMetrica);
CREATE INDEX IX_Metricas_DataCriacao ON Metricas(DataCriacao);
```

### **10. Tabela: Relatorios**

**Descrição:** Armazena configurações de relatórios e agendamentos.

```sql
CREATE TABLE Relatorios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nome NVARCHAR(200) NOT NULL,
    Tipo NVARCHAR(50) NOT NULL, -- Performance, Volume, Satisfacao, Customizado
    Descricao NVARCHAR(500) NULL,
    Parametros NVARCHAR(MAX) NULL, -- JSON com parâmetros do relatório
    DataInicio DATE NOT NULL,
    DataFim DATE NOT NULL,
    DataCriacao DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CriadoPorId INT NOT NULL,
    Agendado BIT NOT NULL DEFAULT 0,
    Frequencia NVARCHAR(20) NULL, -- Diario, Semanal, Mensal
    ProximaExecucao DATETIME2 NULL,
    UltimaExecucao DATETIME2 NULL,
    Destinatarios NVARCHAR(MAX) NULL, -- JSON com lista de emails
    Formato NVARCHAR(20) NOT NULL DEFAULT 'PDF', -- PDF, Excel, CSV
    Ativo BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Relatorios_CriadoPor FOREIGN KEY (CriadoPorId) REFERENCES Usuarios(Id),
    CONSTRAINT CK_TipoRelatorio CHECK (Tipo IN ('Performance', 'Volume', 'Satisfacao', 'Customizado')),
    CONSTRAINT CK_FrequenciaRelatorio CHECK (Frequencia IN ('Diario', 'Semanal', 'Mensal')),
    CONSTRAINT CK_FormatoRelatorio CHECK (Formato IN ('PDF', 'Excel', 'CSV'))
);

-- Índices
CREATE INDEX IX_Relatorios_CriadoPorId ON Relatorios(CriadoPorId);
CREATE INDEX IX_Relatorios_Agendado ON Relatorios(Agendado);
CREATE INDEX IX_Relatorios_ProximaExecucao ON Relatorios(ProximaExecucao);
CREATE INDEX IX_Relatorios_Tipo ON Relatorios(Tipo);
```

### **11. Tabela: ConfiguracoesNotificacao**

**Descrição:** Armazena configurações de notificação por usuário.

```sql
CREATE TABLE ConfiguracoesNotificacao (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UsuarioId INT NOT NULL,
    EmailAtivo BIT NOT NULL DEFAULT 1,
    PushAtivo BIT NOT NULL DEFAULT 1,
    InAppAtivo BIT NOT NULL DEFAULT 1,
    NotificarStatus BIT NOT NULL DEFAULT 1,
    NotificarAtribuicao BIT NOT NULL DEFAULT 1,
    NotificarResolucao BIT NOT NULL DEFAULT 1,
    NotificarComentarios BIT NOT NULL DEFAULT 1,
    NotificarEscalacao BIT NOT NULL DEFAULT 1,
    Frequencia NVARCHAR(20) NOT NULL DEFAULT 'Imediato', -- Imediato, Diario, Semanal
    HorarioSilenciosoInicio TIME NULL,
    HorarioSilenciosoFim TIME NULL,
    DiasSemana NVARCHAR(20) NULL, -- JSON com dias da semana
    DataCriacao DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DataAtualizacao DATETIME2 NULL,
    CONSTRAINT FK_ConfiguracoesNotificacao_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT CK_FrequenciaNotificacao CHECK (Frequencia IN ('Imediato', 'Diario', 'Semanal')),
    CONSTRAINT UQ_ConfiguracoesNotificacao_Usuario UNIQUE (UsuarioId)
);

-- Índices
CREATE INDEX IX_ConfiguracoesNotificacao_UsuarioId ON ConfiguracoesNotificacao(UsuarioId);
```

### **12. Tabela: HistoricoNotificacoes**

**Descrição:** Armazena histórico de envio de notificações para auditoria e análise.

```sql
CREATE TABLE HistoricoNotificacoes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    NotificacaoId INT NOT NULL,
    UsuarioId INT NOT NULL,
    Tipo NVARCHAR(50) NOT NULL,
    Canal NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL, -- Enviada, Falhou, Entregue, Lida
    DataEnvio DATETIME2 NOT NULL,
    DataEntrega DATETIME2 NULL,
    DataLeitura DATETIME2 NULL,
    Erro NVARCHAR(500) NULL,
    Tentativas INT NOT NULL DEFAULT 1,
    Provider NVARCHAR(50) NULL, -- SendGrid, Firebase, etc.
    ProviderId NVARCHAR(100) NULL, -- ID do provider
    CONSTRAINT FK_HistoricoNotificacoes_Notificacao FOREIGN KEY (NotificacaoId) REFERENCES Notificacoes(Id),
    CONSTRAINT FK_HistoricoNotificacoes_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT CK_StatusNotificacao CHECK (Status IN ('Enviada', 'Falhou', 'Entregue', 'Lida')),
    CONSTRAINT CK_CanalHistorico CHECK (Canal IN ('Email', 'Push', 'InApp', 'SMS'))
);

-- Índices
CREATE INDEX IX_HistoricoNotificacoes_NotificacaoId ON HistoricoNotificacoes(NotificacaoId);
CREATE INDEX IX_HistoricoNotificacoes_UsuarioId ON HistoricoNotificacoes(UsuarioId);
CREATE INDEX IX_HistoricoNotificacoes_Status ON HistoricoNotificacoes(Status);
CREATE INDEX IX_HistoricoNotificacoes_DataEnvio ON HistoricoNotificacoes(DataEnvio);
```

### **13. Tabela: DashboardWidgets**

**Descrição:** Armazena configurações de widgets do dashboard para personalização.

```sql
CREATE TABLE DashboardWidgets (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UsuarioId INT NOT NULL,
    Nome NVARCHAR(100) NOT NULL,
    Tipo NVARCHAR(50) NOT NULL, -- Grafico, Tabela, KPI, Lista
    Configuracao NVARCHAR(MAX) NOT NULL, -- JSON com configuração do widget
    PosicaoX INT NOT NULL DEFAULT 0,
    PosicaoY INT NOT NULL DEFAULT 0,
    Largura INT NOT NULL DEFAULT 4,
    Altura INT NOT NULL DEFAULT 3,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCriacao DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DataAtualizacao DATETIME2 NULL,
    CONSTRAINT FK_DashboardWidgets_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT CK_TipoWidget CHECK (Tipo IN ('Grafico', 'Tabela', 'KPI', 'Lista'))
);

-- Índices
CREATE INDEX IX_DashboardWidgets_UsuarioId ON DashboardWidgets(UsuarioId);
CREATE INDEX IX_DashboardWidgets_Ativo ON DashboardWidgets(Ativo);
```

## 🔗 **Relacionamentos entre Tabelas**

### **Diagrama de Relacionamentos**

```
Usuarios (1) ──┐
               ├── (N) Chamados
Usuarios (1) ──┘

Chamados (1) ── (N) Comentarios
Chamados (1) ── (N) Notificacoes
Chamados (1) ── (N) InteracoesIA

Usuarios (1) ── (N) Comentarios
Usuarios (1) ── (N) Notificacoes
Usuarios (1) ── (N) InteracoesIA
Usuarios (1) ── (N) FAQ
Usuarios (1) ── (N) LogsAuditoria
Usuarios (1) ── (N) ConfiguracoesSistema
Usuarios (1) ── (N) Relatorios
Usuarios (1) ── (1) ConfiguracoesNotificacao
Usuarios (1) ── (N) DashboardWidgets

Chamados (1) ── (N) HistoricoNotificacoes
Notificacoes (1) ── (N) HistoricoNotificacoes

Usuarios (1) ── (N) Usuarios (CriadoPorId)
```

### **Relacionamentos Detalhados**

1. **Usuarios → Chamados (1:N)**
   - Um usuário pode ter vários chamados
   - Um chamado pertence a um usuário

2. **Usuarios → Chamados (1:N) - Técnico**
   - Um técnico pode atender vários chamados
   - Um chamado pode ser atribuído a um técnico

3. **Chamados → Comentarios (1:N)**
   - Um chamado pode ter vários comentários
   - Um comentário pertence a um chamado

4. **Usuarios → Comentarios (1:N)**
   - Um usuário pode fazer vários comentários
   - Um comentário é feito por um usuário

5. **Usuarios → Notificacoes (1:N)**
   - Um usuário pode receber várias notificações
   - Uma notificação é enviada para um usuário

6. **Chamados → Notificacoes (1:N)**
   - Um chamado pode gerar várias notificações
   - Uma notificação pode estar relacionada a um chamado

7. **Usuarios → InteracoesIA (1:N)**
   - Um usuário pode ter várias interações com IA
   - Uma interação é feita por um usuário

8. **Chamados → InteracoesIA (1:N)**
   - Um chamado pode ter várias interações com IA
   - Uma interação pode estar relacionada a um chamado

9. **Usuarios → Relatorios (1:N)**
   - Um usuário pode criar vários relatórios
   - Um relatório é criado por um usuário

10. **Usuarios → ConfiguracoesNotificacao (1:1)**
    - Um usuário tem uma configuração de notificação
    - Uma configuração pertence a um usuário

11. **Usuarios → DashboardWidgets (1:N)**
    - Um usuário pode ter vários widgets no dashboard
    - Um widget pertence a um usuário

12. **Chamados → HistoricoNotificacoes (1:N)**
    - Um chamado pode gerar várias entradas no histórico
    - Uma entrada do histórico pode estar relacionada a um chamado

13. **Notificacoes → HistoricoNotificacoes (1:N)**
    - Uma notificação pode ter várias tentativas de envio
    - Uma entrada do histórico pertence a uma notificação

## 📊 **Views e Procedures**

### **View: ChamadosCompleto**
```sql
CREATE VIEW vw_ChamadosCompleto AS
SELECT 
    c.Id,
    c.Titulo,
    c.Descricao,
    c.Status,
    c.Prioridade,
    c.Categoria,
    c.Subcategoria,
    c.DataCriacao,
    c.DataResolucao,
    c.TempoResolucao,
    c.Satisfacao,
    u.Nome AS UsuarioNome,
    u.Email AS UsuarioEmail,
    t.Nome AS TecnicoNome,
    t.Email AS TecnicoEmail,
    DATEDIFF(MINUTE, c.DataCriacao, ISNULL(c.DataResolucao, GETUTCDATE())) AS TempoDecorrido
FROM Chamados c
INNER JOIN Usuarios u ON c.UsuarioId = u.Id
LEFT JOIN Usuarios t ON c.TecnicoId = t.Id;
```

### **View: EstatisticasChamados**
```sql
CREATE VIEW vw_EstatisticasChamados AS
SELECT 
    COUNT(*) AS TotalChamados,
    COUNT(CASE WHEN Status = 'Aberto' THEN 1 END) AS ChamadosAbertos,
    COUNT(CASE WHEN Status = 'Em Andamento' THEN 1 END) AS ChamadosEmAndamento,
    COUNT(CASE WHEN Status = 'Resolvido' THEN 1 END) AS ChamadosResolvidos,
    COUNT(CASE WHEN Status = 'Fechado' THEN 1 END) AS ChamadosFechados,
    AVG(CAST(TempoResolucao AS FLOAT)) AS TempoMedioResolucao,
    AVG(CAST(Satisfacao AS FLOAT)) AS SatisfacaoMedia,
    COUNT(CASE WHEN DataCriacao >= DATEADD(DAY, -30, GETUTCDATE()) THEN 1 END) AS ChamadosUltimos30Dias
FROM Chamados;
```

### **View: MetricasDashboard**
```sql
CREATE VIEW vw_MetricasDashboard AS
SELECT 
    DataMetrica,
    TipoMetrica,
    ChamadosAbertos,
    ChamadosResolvidos,
    ChamadosFechados,
    TempoMedioResolucao,
    SatisfacaoMedia,
    UsuariosAtivos,
    TecnicosAtivos,
    TaxaResolucaoPrimeiroContato,
    ROW_NUMBER() OVER (ORDER BY DataMetrica DESC) AS OrdemRecente
FROM Metricas
WHERE DataMetrica >= DATEADD(DAY, -30, GETUTCDATE());
```

### **View: NotificacoesResumo**
```sql
CREATE VIEW vw_NotificacoesResumo AS
SELECT 
    n.Id,
    n.Titulo,
    n.Tipo,
    n.Prioridade,
    n.Lida,
    n.DataEnvio,
    n.DataLeitura,
    u.Nome AS UsuarioNome,
    u.Email AS UsuarioEmail,
    c.Titulo AS ChamadoTitulo,
    DATEDIFF(MINUTE, n.DataEnvio, ISNULL(n.DataLeitura, GETUTCDATE())) AS TempoParaLeitura
FROM Notificacoes n
INNER JOIN Usuarios u ON n.UsuarioId = u.Id
LEFT JOIN Chamados c ON n.ChamadoId = c.Id;
```

### **View: RelatoriosAgendados**
```sql
CREATE VIEW vw_RelatoriosAgendados AS
SELECT 
    r.Id,
    r.Nome,
    r.Tipo,
    r.Frequencia,
    r.ProximaExecucao,
    r.UltimaExecucao,
    r.Ativo,
    u.Nome AS CriadoPorNome,
    DATEDIFF(HOUR, ISNULL(r.UltimaExecucao, r.DataCriacao), GETUTCDATE()) AS HorasDesdeUltimaExecucao
FROM Relatorios r
INNER JOIN Usuarios u ON r.CriadoPorId = u.Id
WHERE r.Agendado = 1 AND r.Ativo = 1;
```

### **Procedure: sp_CriarChamado**
```sql
CREATE PROCEDURE sp_CriarChamado
    @Titulo NVARCHAR(200),
    @Descricao NVARCHAR(2000),
    @Prioridade NVARCHAR(50),
    @UsuarioId INT,
    @ChamadoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Chamados (Titulo, Descricao, Prioridade, UsuarioId)
    VALUES (@Titulo, @Descricao, @Prioridade, @UsuarioId);
    
    SET @ChamadoId = SCOPE_IDENTITY();
    
    -- Log de auditoria
    INSERT INTO LogsAuditoria (UsuarioId, Acao, Tabela, RegistroId, DadosNovos)
    VALUES (@UsuarioId, 'INSERT', 'Chamados', @ChamadoId, 
            JSON_OBJECT('Titulo', @Titulo, 'Prioridade', @Prioridade));
END;
```

### **Procedure: sp_AtualizarStatusChamado**
```sql
CREATE PROCEDURE sp_AtualizarStatusChamado
    @ChamadoId INT,
    @NovoStatus NVARCHAR(50),
    @TecnicoId INT,
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @StatusAnterior NVARCHAR(50);
    SELECT @StatusAnterior = Status FROM Chamados WHERE Id = @ChamadoId;
    
    UPDATE Chamados 
    SET Status = @NovoStatus,
        DataAtualizacao = GETUTCDATE(),
        TecnicoId = @TecnicoId,
        DataResolucao = CASE WHEN @NovoStatus = 'Resolvido' THEN GETUTCDATE() ELSE DataResolucao END
    WHERE Id = @ChamadoId;
    
    -- Log de auditoria
    INSERT INTO LogsAuditoria (UsuarioId, Acao, Tabela, RegistroId, DadosAntigos, DadosNovos)
    VALUES (@UsuarioId, 'UPDATE', 'Chamados', @ChamadoId,
            JSON_OBJECT('Status', @StatusAnterior),
            JSON_OBJECT('Status', @NovoStatus, 'TecnicoId', @TecnicoId));
END;
```

### **Procedure: sp_CalcularMetricasDiarias**
```sql
CREATE PROCEDURE sp_CalcularMetricasDiarias
    @DataMetrica DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ChamadosAbertos INT, @ChamadosResolvidos INT, @ChamadosFechados INT;
    DECLARE @TempoMedioResolucao DECIMAL(10,2), @SatisfacaoMedia DECIMAL(3,2);
    DECLARE @UsuariosAtivos INT, @TecnicosAtivos INT;
    DECLARE @TaxaResolucaoPrimeiroContato DECIMAL(5,2);
    
    -- Calcular métricas básicas
    SELECT @ChamadosAbertos = COUNT(*) FROM Chamados 
    WHERE CAST(DataCriacao AS DATE) = @DataMetrica;
    
    SELECT @ChamadosResolvidos = COUNT(*) FROM Chamados 
    WHERE CAST(DataResolucao AS DATE) = @DataMetrica;
    
    SELECT @ChamadosFechados = COUNT(*) FROM Chamados 
    WHERE CAST(DataAtualizacao AS DATE) = @DataMetrica AND Status = 'Fechado';
    
    SELECT @TempoMedioResolucao = AVG(CAST(TempoResolucao AS DECIMAL(10,2))) 
    FROM Chamados WHERE CAST(DataResolucao AS DATE) = @DataMetrica;
    
    SELECT @SatisfacaoMedia = AVG(CAST(Satisfacao AS DECIMAL(3,2))) 
    FROM Chamados WHERE CAST(DataResolucao AS DATE) = @DataMetrica AND Satisfacao IS NOT NULL;
    
    SELECT @UsuariosAtivos = COUNT(DISTINCT UsuarioId) FROM Chamados 
    WHERE CAST(DataCriacao AS DATE) = @DataMetrica;
    
    SELECT @TecnicosAtivos = COUNT(DISTINCT TecnicoId) FROM Chamados 
    WHERE CAST(DataAtualizacao AS DATE) = @DataMetrica AND TecnicoId IS NOT NULL;
    
    -- Inserir métricas calculadas
    INSERT INTO Metricas (
        DataMetrica, HoraMetrica, TipoMetrica, ChamadosAbertos, ChamadosResolvidos, 
        ChamadosFechados, TempoMedioResolucao, SatisfacaoMedia, UsuariosAtivos, 
        TecnicosAtivos, TaxaResolucaoPrimeiroContato
    )
    VALUES (
        @DataMetrica, CAST(GETUTCDATE() AS TIME), 'Diaria', @ChamadosAbertos, 
        @ChamadosResolvidos, @ChamadosFechados, @TempoMedioResolucao, 
        @SatisfacaoMedia, @UsuariosAtivos, @TecnicosAtivos, @TaxaResolucaoPrimeiroContato
    );
END;
```

### **Procedure: sp_EnviarNotificacao**
```sql
CREATE PROCEDURE sp_EnviarNotificacao
    @UsuarioId INT,
    @Tipo NVARCHAR(50),
    @Titulo NVARCHAR(200),
    @Mensagem NVARCHAR(1000),
    @Prioridade NVARCHAR(20),
    @Canal NVARCHAR(50),
    @ChamadoId INT NULL,
    @Acao NVARCHAR(100) NULL,
    @NotificacaoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verificar configurações do usuário
    IF EXISTS (
        SELECT 1 FROM ConfiguracoesNotificacao 
        WHERE UsuarioId = @UsuarioId AND 
        ((@Canal = 'Email' AND EmailAtivo = 0) OR 
         (@Canal = 'InApp' AND InAppAtivo = 0) OR
         (@Canal = 'Push' AND PushAtivo = 0))
    )
    BEGIN
        SET @NotificacaoId = -1; -- Usuário optou por não receber
        RETURN;
    END
    
    -- Inserir notificação
    INSERT INTO Notificacoes (
        UsuarioId, Tipo, Titulo, Mensagem, Prioridade, Canal, ChamadoId, Acao
    )
    VALUES (
        @UsuarioId, @Tipo, @Titulo, @Mensagem, @Prioridade, @Canal, @ChamadoId, @Acao
    );
    
    SET @NotificacaoId = SCOPE_IDENTITY();
    
    -- Registrar no histórico
    INSERT INTO HistoricoNotificacoes (
        NotificacaoId, UsuarioId, Tipo, Canal, Status, DataEnvio
    )
    VALUES (
        @NotificacaoId, @UsuarioId, @Tipo, @Canal, 'Enviada', GETUTCDATE()
    );
END;
```

### **Procedure: sp_ProcessarRelatoriosAgendados**
```sql
CREATE PROCEDURE sp_ProcessarRelatoriosAgendados
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RelatorioId INT, @Nome NVARCHAR(200), @Tipo NVARCHAR(50);
    DECLARE @Parametros NVARCHAR(MAX), @Destinatarios NVARCHAR(MAX);
    DECLARE @Formato NVARCHAR(20), @CriadoPorId INT;
    
    DECLARE relatorios_cursor CURSOR FOR
    SELECT Id, Nome, Tipo, Parametros, Destinatarios, Formato, CriadoPorId
    FROM Relatorios 
    WHERE Agendado = 1 AND Ativo = 1 
    AND ProximaExecucao <= GETUTCDATE();
    
    OPEN relatorios_cursor;
    FETCH NEXT FROM relatorios_cursor INTO @RelatorioId, @Nome, @Tipo, @Parametros, @Destinatarios, @Formato, @CriadoPorId;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Aqui seria a lógica de geração do relatório
        -- Por enquanto, apenas atualizamos as datas
        
        UPDATE Relatorios 
        SET UltimaExecucao = GETUTCDATE(),
            ProximaExecucao = CASE 
                WHEN Frequencia = 'Diario' THEN DATEADD(DAY, 1, GETUTCDATE())
                WHEN Frequencia = 'Semanal' THEN DATEADD(WEEK, 1, GETUTCDATE())
                WHEN Frequencia = 'Mensal' THEN DATEADD(MONTH, 1, GETUTCDATE())
                ELSE NULL
            END
        WHERE Id = @RelatorioId;
        
        FETCH NEXT FROM relatorios_cursor INTO @RelatorioId, @Nome, @Tipo, @Parametros, @Destinatarios, @Formato, @CriadoPorId;
    END
    
    CLOSE relatorios_cursor;
    DEALLOCATE relatorios_cursor;
END;
```

## 🔐 **Segurança e Permissões**

### **Usuários do Banco**
```sql
-- Usuário da aplicação
CREATE USER [helpfast_app] FOR LOGIN [helpfast_app];
ALTER ROLE db_datareader ADD MEMBER [helpfast_app];
ALTER ROLE db_datawriter ADD MEMBER [helpfast_app];

-- Usuário de backup
CREATE USER [helpfast_backup] FOR LOGIN [helpfast_backup];
ALTER ROLE db_backupoperator ADD MEMBER [helpfast_backup];
```

### **Políticas de Segurança**
```sql
-- Row Level Security para dados sensíveis
CREATE SECURITY POLICY sp_UsuariosSeguranca
ADD FILTER PREDICATE dbo.fn_UsuariosSeguranca(Id) ON dbo.Usuarios;

-- Função de segurança
CREATE FUNCTION dbo.fn_UsuariosSeguranca(@UsuarioId INT)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS fn_securitypredicate_result
WHERE @UsuarioId = USER_ID() OR IS_ROLEMEMBER('db_owner') = 1;
```

## 📈 **Índices e Performance**

### **Índices Compostos**
```sql
-- Índice para consultas de chamados por usuário e status
CREATE INDEX IX_Chamados_Usuario_Status ON Chamados(UsuarioId, Status);

-- Índice para consultas de chamados por técnico e data
CREATE INDEX IX_Chamados_Tecnico_Data ON Chamados(TecnicoId, DataCriacao);

-- Índice para consultas de notificações não lidas
CREATE INDEX IX_Notificacoes_Usuario_Lida ON Notificacoes(UsuarioId, Lida);
```

### **Índices de Texto Completo**
```sql
-- Índice de texto completo para busca em chamados
CREATE FULLTEXT CATALOG ft_Chamados;
CREATE FULLTEXT INDEX ON Chamados(Titulo, Descricao) 
KEY INDEX PK_Chamados ON ft_Chamados;

-- Índice de texto completo para busca em FAQ
CREATE FULLTEXT INDEX ON FAQ(Titulo, Descricao, Solucao) 
KEY INDEX PK_FAQ ON ft_Chamados;
```

## 🔄 **Manutenção e Backup**

### **Jobs de Manutenção**
```sql
-- Job para limpeza de logs antigos (manter 7 anos)
CREATE PROCEDURE sp_LimparLogsAntigos
AS
BEGIN
    DELETE FROM LogsAuditoria 
    WHERE DataAcao < DATEADD(YEAR, -7, GETUTCDATE());
END;

-- Job para atualização de estatísticas
CREATE PROCEDURE sp_AtualizarEstatisticas
AS
BEGIN
    UPDATE STATISTICS Chamados;
    UPDATE STATISTICS Usuarios;
    UPDATE STATISTICS Notificacoes;
    UPDATE STATISTICS Metricas;
    UPDATE STATISTICS Relatorios;
    UPDATE STATISTICS HistoricoNotificacoes;
END;

-- Job para cálculo de métricas diárias
CREATE PROCEDURE sp_ExecutarCalculoMetricas
AS
BEGIN
    DECLARE @DataOntem DATE = DATEADD(DAY, -1, CAST(GETUTCDATE() AS DATE));
    EXEC sp_CalcularMetricasDiarias @DataOntem;
END;

-- Job para processamento de relatórios agendados
CREATE PROCEDURE sp_ExecutarRelatoriosAgendados
AS
BEGIN
    EXEC sp_ProcessarRelatoriosAgendados;
END;
```

### **Política de Retenção**
- **Logs de Auditoria:** 7 anos
- **Notificações Lidas:** 1 ano
- **Histórico de Notificações:** 2 anos
- **Interações IA:** 2 anos
- **Chamados Fechados:** 5 anos
- **Métricas Diárias:** 2 anos
- **Métricas Semanais:** 5 anos
- **Métricas Mensais:** 10 anos
- **Relatórios Agendados:** 1 ano após inativação

## 📝 **Scripts de Criação**

### **Script Completo de Criação**
```sql
-- 1. Criar tabelas
-- 2. Criar índices
-- 3. Criar views
-- 4. Criar procedures
-- 5. Criar usuários
-- 6. Configurar segurança
-- 7. Inserir dados iniciais
```

### **Dados Iniciais**
```sql
-- Usuário administrador master
INSERT INTO Usuarios (Nome, Email, Telefone, Senha, TipoUsuario, CriadoPorId)
VALUES ('Administrador', 'admin@helpfast.com', '(11) 99999-9999', 
        '$2a$10$N9qo8uLOickgx2ZMRZoMye', 3, NULL);

-- Configurações iniciais
INSERT INTO ConfiguracoesSistema (Chave, Valor, Tipo, Descricao, AtualizadoPorId)
VALUES 
('sistema_nome', 'HelpFast', 'String', 'Nome do sistema', 1),
('sistema_versao', '1.0.0', 'String', 'Versão atual do sistema', 1),
('email_smtp_host', 'smtp.gmail.com', 'String', 'Servidor SMTP', 1),
('openai_model', 'gpt-4', 'String', 'Modelo OpenAI', 1);
```

## 📊 **Monitoramento**

### **Queries de Monitoramento**
```sql
-- Chamados por status
SELECT Status, COUNT(*) as Quantidade
FROM Chamados
GROUP BY Status;

-- Performance de resolução
SELECT 
    Prioridade,
    AVG(CAST(TempoResolucao AS FLOAT)) as TempoMedio,
    COUNT(*) as Total
FROM Chamados
WHERE Status = 'Fechado'
GROUP BY Prioridade;

-- Satisfação dos usuários
SELECT 
    AVG(CAST(Satisfacao AS FLOAT)) as SatisfacaoMedia,
    COUNT(*) as TotalAvaliacoes
FROM Chamados
WHERE Satisfacao IS NOT NULL;
```

## 🚀 **Considerações de Escalabilidade**

### **Particionamento**
- **Chamados:** Por data de criação (mensal)
- **Logs:** Por data de ação (anual)
- **Notificações:** Por data de envio (mensal)

### **Arquivamento**
- Chamados fechados há mais de 2 anos
- Logs de auditoria antigos
- Notificações lidas antigas

### **Replicação**
- Read replicas para consultas
- Backup automático diário
- Point-in-time recovery

## 📝 **Notas Importantes**

- **Conformidade LGPD:** Todos os dados pessoais são criptografados
- **Backup:** Automático com retenção de 7 dias
- **Monitoramento:** Logs de todas as operações
- **Segurança:** Row Level Security implementada
- **Performance:** Índices otimizados para consultas frequentes
- **Escalabilidade:** Preparado para crescimento de dados
