# Arquitetura do Sistema HelpFast

## 📋 **Visão Geral**

O HelpFast é um sistema multiplataforma de gerenciamento de chamados que utiliza uma arquitetura moderna baseada em microsserviços, com componentes distribuídos entre Azure Cloud e Oracle Cloud para máxima eficiência e escalabilidade.

## ☁️ **Infraestrutura Cloud**

### **Azure Cloud (Free Tier)**
- **Azure SQL Database:** Banco de dados principal

### **Oracle Cloud (Free Tier)**
- **Java API:** API de integração com IA e notificações
- **OpenAI Integration:** Processamento de IA via Java API
- **Email Service:** Serviço de notificações por email
- **Oracle Container Engine:** Deploy de containers

## 🗄️ **Banco de Dados**

### **Azure SQL Database (Principal)**
```sql
-- Tabelas Principais
CREATE TABLE Usuarios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nome NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Telefone NVARCHAR(15) NOT NULL,
    Senha NVARCHAR(255) NOT NULL,
    TipoUsuario INT NOT NULL,
    DataCriacao DATETIME2 NOT NULL,
    UltimoLogin DATETIME2,
    Ativo BIT NOT NULL DEFAULT 1,
    CriadoPorId INT NULL,
    FOREIGN KEY (CriadoPorId) REFERENCES Usuarios(Id)
);

CREATE TABLE Chamados (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Titulo NVARCHAR(100) NOT NULL,
    Descricao NVARCHAR(1000) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Aberto',
    Prioridade NVARCHAR(50) NOT NULL DEFAULT 'Media',
    DataCriacao DATETIME2 NOT NULL,
    DataAtualizacao DATETIME2,
    DataResolucao DATETIME2,
    UsuarioId INT NOT NULL,
    TecnicoId INT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    FOREIGN KEY (TecnicoId) REFERENCES Usuarios(Id)
);

CREATE TABLE Notificacoes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UsuarioId INT NOT NULL,
    Tipo NVARCHAR(50) NOT NULL,
    Titulo NVARCHAR(200) NOT NULL,
    Mensagem NVARCHAR(500) NOT NULL,
    Lida BIT NOT NULL DEFAULT 0,
    DataEnvio DATETIME2 NOT NULL,
    DataLeitura DATETIME2,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
```

### **Estrutura de Dados**
- **Usuarios:** Gestão de usuários e perfis
- **Chamados:** Tickets de suporte
- **Comentarios:** Histórico de conversas
- **Notificacoes:** Sistema de notificações
- **FAQ:** Perguntas frequentes
- **Logs:** Auditoria do sistema

## 🔧 **Componentes Técnicos**

### **1. HelpFast WebApp (ASP.NET Core)**
- **Framework:** ASP.NET Core 8.0
- **Banco de Dados:** Conexão direta com Azure SQL Database
- **Integração:** Comunicação HTTP com Java API
- **Funcionalidades:** Interface web, gerenciamento de chamados, autenticação

### **2. Java API (Oracle Cloud)**
- **Framework:** Spring Boot
- **Integração:** OpenAI API para processamento de IA
- **Funcionalidades:** Categorização automática, atribuição automática, notificações por email
- **Contrato:** API REST com documentação Swagger

### **3. Desktop App (WinForms.NET)**
- **Framework:** .NET 8.0 WinForms
- **Banco de Dados:** Conexão direta com Azure SQL Database
- **Integração:** Comunicação HTTP com Java API
- **Funcionalidades:** Interface desktop, gerenciamento de chamados, autenticação

### **4. Mobile App (Java Android)**
- **Framework:** Java Android
- **Banco Local:** SQLite para cache offline
- **Sincronização:** Azure SQL Database via API
- **Integração:** Comunicação HTTP com Java API
- **Funcionalidades:** Interface mobile, gerenciamento de chamados, sincronização offline

## 🔌 **Integrações Externas**

### **OpenAI API**
- **Modelo:** GPT-4
- **Funcionalidades:** Categorização automática, análise de contexto, chat inteligente
- **Integração:** Via Java API

### **Serviços de Email**
- **REST:** Configuração para envio de notificações
- **Templates:** Templates HTML para diferentes tipos de notificação
- **Integração:** Via Java API

## 🔐 **Segurança e Autenticação**

### **Autenticação JWT**
- **Token:** JWT para autenticação entre plataformas
- **Expiração:** 8 horas
- **Claims:** Informações do usuário e permissões

### **Criptografia de Senhas**
- **Algoritmo:** BCrypt
- **Salt:** Geração automática de salt
- **Verificação:** Validação segura de senhas

## 📊 **Monitoramento e Logs**

### **Estrutura de Logs**
- **Formato:** JSON estruturado
- **Campos:** Timestamp, nível, serviço, mensagem, usuário, IP, correlation ID
- **Retenção:** 7 anos para auditoria

### **Métricas de Performance**
- **Tempo de Resposta:** < 2 segundos
- **Disponibilidade:** 99.9% uptime
- **Throughput:** 100 requests/second
- **Error Rate:** < 0.1%

## 🚀 **Deploy e DevOps**

### **Pipeline CI/CD**
- **GitHub Actions:** Deploy automático
- **Branches:** Main branch trigger
- **Ambientes:** Desenvolvimento, teste, produção

### **Containerização**
- **Docker:** Containerização dos serviços
- **Base Images:** .NET 8.0, Java 17
- **Orquestração:** Docker Compose para desenvolvimento

## 📱 **Multiplataforma**

### **Web App (ASP.NET Core)**
- **Frontend:** Razor Pages + JavaScript
- **Backend:** ASP.NET Core Web API
- **Database:** Entity Framework Core
- **Deploy:** Oracle Cloud

### **Desktop App (WinForms.NET)**
- **Framework:** .NET 8.0 WinForms
- **Database:** Entity Framework Core
- **Deploy:** Executável Windows

### **Mobile App (Java Android)**
- **Framework:** Java Android
- **Backend:** REST API
- **Database:** SQLite local + API sync

## 🔄 **Comunicação entre Componentes**

### **Fluxo de Comunicação Direta**

#### **1. Acesso às Plataformas:**
- **Web App (ASP.NET Core):** Acesso direto ao Azure SQL Database
- **Mobile App (Java Android):** Acesso direto ao Azure SQL Database
- **Desktop App (C#):** Acesso direto ao Azure SQL Database

#### **2. Integração com IA:**
- **Todas as plataformas** se comunicam diretamente com **Java API**
- **Java API** integra com **OpenAI API** para processamento de IA
- **Java API** é responsável por:
  - Categorização automática de chamados
  - Atribuição automática de chamados
  - Análise de padrões e recorrência

#### **3. Sistema de Notificações:**
- **Java API** é responsável por enviar notificações via email
- **Notificações** são disparadas quando há alteração de status
- **Email Service** integrado ao Java API

### **Comunicação - Web App**
- **Banco de Dados:** Acesso direto ao Azure SQL Database
- **Java API:** Chamadas HTTP para processamento de IA
- **Fluxo:** Salva chamado → Chama Java API → Recebe resposta

### **Comunicação - Java API**
- **OpenAI:** Integração para categorização e análise
- **Email:** Envio de notificações automáticas
- **Fluxo:** Recebe chamado → Processa IA → Atribui técnico → Envia notificação

## 📊 **Fluxo de Dados Detalhado**

### **1. Criação de Chamado:**
```
Cliente → Plataforma (Web/Mobile/Desktop) → Azure SQL Database
                ↓
         Java API (categorização) → OpenAI API
                ↓
         Atribuição automática → Azure SQL Database
                ↓
         Email Service → Notificação para técnico
```

### **2. Processamento com IA:**
```
Chamado → Java API → OpenAI API → Análise → Categoria → Atribuição → Notificação
```

### **3. Consulta de Dados:**
```
Plataforma → Azure SQL Database (acesso direto)
```

### **4. Notificações:**
```
Mudança Status → Java API → Email Service → Cliente/Técnico
```

## 🔄 **Comunicação em Tempo Real**

### **SignalR (Futuro)**
- **Tempo Real:** Notificações instantâneas
- **Grupos:** Organização por usuário
- **Funcionalidades:** Chat, notificações push, atualizações em tempo real

## 📈 **Escalabilidade**

### **Horizontal Scaling**
- **Load Balancer:** Distribuição de carga
- **Database Sharding:** Particionamento de dados
- **Caching:** Redis para performance
- **CDN:** Distribuição de conteúdo estático

### **Vertical Scaling**
- **Resource Optimization:** Otimização de recursos
- **Database Indexing:** Índices otimizados
- **Query Optimization:** Consultas eficientes
- **Memory Management:** Gerenciamento de memória

## 🛡️ **Conformidade e Segurança**

### **LGPD Compliance**
- **Data Minimization:** Dados mínimos necessários
- **Consent Management:** Controle de consentimento
- **Data Retention:** Política de retenção
- **Right to be Forgotten:** Direito ao esquecimento

### **Security Best Practices**
- **HTTPS Everywhere:** Comunicação segura
- **Input Validation:** Validação de entrada
- **SQL Injection Prevention:** Prevenção de ataques
- **Rate Limiting:** Controle de taxa de requisições

## 📝 **Notas Importantes**

- **Cloud Agnostic:** Preparado para migração entre clouds
- **Microservices Ready:** Arquitetura preparada para microsserviços
- **API First:** Todas as funcionalidades expostas via API
- **Event Driven:** Comunicação baseada em eventos
- **Observability:** Monitoramento completo do sistema
