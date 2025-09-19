# Diagrama de Componentes - HelpFast

## 🏗️ **Arquitetura de Componentes**

Conforme especificado nos requisitos do sistema, o HelpFast utiliza os seguintes componentes:

### **Azure Cloud (Free Tier)**
```
┌─────────────────────────────────┐
│         Azure Cloud             │
│                                 │
│  ┌─────────────────────────────┐│
│  │    Azure SQL Database       ││
│  │                             ││
│  │  • Banco de dados relacional││
│  │  • Armazena todas as        ││
│  │    informações do sistema   ││
│  │  • Usuários, chamados,      ││
│  │    notificações, logs       ││
│  └─────────────────────────────┘│
└─────────────────────────────────┘
```

### **Oracle Cloud (Free Tier)**
```
┌─────────────────────────────────┐
│        Oracle Cloud             │
│                                 │
│  ┌─────────────────────────────┐│
│  │    HelpFast WebApp          ││
│  │                             ││
│  │  • ASP.NET Core             ││
│  │  • Serviço de aplicação     ││
│  │  • Versão web do sistema    ││
│  │  • Interface responsiva     ││
│  └─────────────────────────────┘│
│                                 │
│  ┌─────────────────────────────┐│
│  │      Java API               ││
│  │                             ││
│  │  • API dedicada para IA     ││
│  │  • Integração OpenAI        ││
│  │  • Centraliza lógica do     ││
│  │    chatBot                  ││
│  │  • Categorização automática ││
│  │  • Atribuição automática    ││
│  └─────────────────────────────┘│
└─────────────────────────────────┘
```

### **Aplicativos Nativos**

#### **HelpFast Mobile App**
```
┌─────────────────────────────────┐
│    HelpFast Mobile App          │
│                                 │
│  • Java Android                 │
│  • Aplicativo mobile nativo     │
│  • Mesmas funcionalidades       │
│  • Gerenciamento de chamados    │
│  • Interface Material Design    │
│  • SQLite local + API sync      │
└─────────────────────────────────┘
```

#### **HelpFast Desktop App**
```
┌─────────────────────────────────┐
│    HelpFast Desktop App         │
│                                 │
│  • C# .NET                      │
│  • Aplicativo desktop nativo    │
│  • WinForms interface           │
│  • Mesmas funcionalidades       │
│  • Gerenciamento de chamados    │
│  • Entity Framework Core        │
└─────────────────────────────────┘
```

## 🔄 **Fluxo de Comunicação**

### **Arquitetura de Componentes**

```
┌─────────────────────────────────────────────────────────────┐
│                Apps and Services available to the user      │
│                                                             │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Web App       │  │   Mobile App    │  │  Desktop App │ │
│  │   Browser       │  │   (Android)     │  │   (C#)       │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
           │                    │                    │
           │                    │                    │
           ▼                    ▼                    ▼
┌─────────────────────────────────────────────────────────────┐
│                    Oracle Cloud                            │
│                                                             │
│  ┌─────────────────┐              ┌─────────────────┐      │
│  │   Web App       │              │   Java API      │      │
│  │   (ASP.NET)     │              │                 │      │
│  └─────────────────┘              └─────────────────┘      │
└─────────────────────────────────────────────────────────────┘
           │                    │                    │
           │                    │                    │
           ▼                    ▼                    ▼
┌─────────────────────────────────────────────────────────────┐
│                    Azure Cloud                             │
│                                                             │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │              Azure SQL Database                        │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
           │
           │
           ▼
┌─────────────────────────────────────────────────────────────┐
│                      Twilio                               │
│                                                             │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │              SendGrid Email API                        │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### **Conexões e Integrações**

| Origem | Destino | Tipo de Conexão | Descrição |
|--------|---------|-----------------|-----------|
| **Web App Browser** | **Web App (ASP.NET)** | Web Connection | Interface web do usuário |
| **Desktop App (C#)** | **Java API** | API Integration | Integração com IA e notificações |
| **Mobile App (Android)** | **Java API** | API Integration | Integração com IA e notificações |
| **Desktop App (C#)** | **Azure SQL Database** | Database Connection | Acesso direto ao banco de dados |
| **Java API** | **Azure SQL Database** | Database Connection | Acesso ao banco para processamento |
| **Web App (ASP.NET)** | **Java API** | API Integration | Integração com IA e notificações |
| **Java API** | **SendGrid Email API** | Send Notification | Envio de notificações por email |

## 🔗 **Integrações Externas**

### **SendGrid Email API (Twilio)**
```
┌─────────────────────────────────┐
│      SendGrid Email API         │
│                                 │
│  • Envio de emails transacionais│
│  • Templates personalizados     │
│  • Relatórios de entrega        │
│  • Análise de engajamento       │
│  • Conformidade com LGPD        │
└─────────────────────────────────┘
                │
                │ HTTP/HTTPS
                │
┌─────────────────────────────────┐
│        Java API                 │
│                                 │
│  • Integração SendGrid          │
│  • Processamento de notificações│
│  • Templates de email           │
│  • Gestão de destinatários      │
└─────────────────────────────────┘
```

### **OpenAI API (Integrada via Java API)**
```
┌─────────────────────────────────┐
│        OpenAI API               │
│                                 │
│  • GPT-4 Model                  │
│  • Processamento de linguagem   │
│  • Análise de contexto          │
│  • Respostas inteligentes       │
│  • Categorização automática     │
└─────────────────────────────────┘
                │
                │ HTTP/HTTPS
                │
┌─────────────────────────────────┐
│        Java API                 │
│                                 │
│  • Integração OpenAI            │
│  • Processamento de chamados    │
│  • Lógica de negócio IA         │
│  • Cache e fallback             │
└─────────────────────────────────┘
```

## 📊 **Fluxo de Dados**

### **1. Criação de Chamado**
```
Cliente → Plataforma (Web/Mobile/Desktop) → Azure SQL Database
                ↓
         Java API (categorização) → OpenAI API
                ↓
         Atribuição automática → Azure SQL Database
                ↓
         SendGrid Email API → Notificação para técnico
```

### **2. Processamento com IA**
```
Chamado → Java API → OpenAI API → Análise → Categoria → Atribuição → SendGrid
```

### **3. Consulta de Dados**
```
Plataforma → Azure SQL Database (acesso direto)
```

### **4. Notificações**
```
Mudança Status → Java API → SendGrid Email API → Cliente/Técnico
```

### **5. Interface Web**
```
Browser → Web App (ASP.NET) → Java API → Azure SQL Database
```

## 🛡️ **Segurança e Conformidade**

### **LGPD Compliance**
- **Azure SQL:** Dados criptografados
- **Oracle Cloud:** Comunicação HTTPS
- **Aplicativos:** Autenticação JWT
- **APIs:** Rate limiting e validação

### **Controle de Acesso**
- **Hierarquia:** Usuário < Técnico < Administrador
- **Permissões:** Baseadas em roles
- **Auditoria:** Logs de todas as operações

## 📈 **Escalabilidade**

### **Horizontal Scaling**
- **Load Balancer:** Distribuição de carga
- **Database Sharding:** Particionamento de dados
- **Caching:** Redis para performance
- **CDN:** Conteúdo estático

### **Vertical Scaling**
- **Resource Optimization:** Otimização de recursos
- **Database Indexing:** Índices otimizados
- **Query Optimization:** Consultas eficientes
- **Memory Management:** Gerenciamento de memória

## 🔧 **Tecnologias por Componente**

| Componente | Tecnologia | Função |
|------------|------------|--------|
| **Azure SQL** | SQL Server | Banco de dados principal |
| **WebApp** | ASP.NET Core | Aplicação web |
| **Java API** | Spring Boot | Integração com IA e notificações |
| **Mobile** | Java Android | App mobile nativo |
| **Desktop** | C# WinForms | App desktop nativo |
| **IA** | OpenAI API | Inteligência artificial |
| **Email** | SendGrid API | Notificações por email |

## 📝 **Notas Importantes**

- **Cloud Agnostic:** Preparado para migração entre clouds
- **Microservices Ready:** Arquitetura preparada para microsserviços
- **API First:** Todas as funcionalidades expostas via API
- **Event Driven:** Comunicação baseada em eventos
- **Observability:** Monitoramento completo do sistema
