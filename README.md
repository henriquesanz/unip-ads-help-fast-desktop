# 📚 Documentação do Sistema HelpFast

## 🎯 **Visão Geral**

O HelpFast é um sistema multiplataforma de gerenciamento de chamados de suporte técnico, desenvolvido para reduzir custos operacionais, melhorar a experiência do cliente e otimizar processos de atendimento através de automação inteligente. O sistema oferece aplicações nativas para Web (ASP.NET Core), Mobile (Java Android) e Desktop (C#), todas integradas através de uma API Java centralizada para inteligência artificial e um banco de dados Azure SQL.

## 📋 **Índice da Documentação**

### **1. 📊 [Sistema de Perfis de Usuários](SISTEMA_PERFIS.md)**
- Estrutura de usuários (Cliente, Técnico, Administrador)
- Hierarquia de permissões
- Regras de negócio para cadastro
- Validações de segurança
- Usuário master: admin@helpfast.com / 123456

### **2. 🎫 [Sistema de Chamados](SISTEMA_CHAMADOS.md)**
- Funcionalidades por tipo de usuário
- Fluxo de trabalho dos chamados
- Estados e prioridades
- Métricas e KPIs
- Interface e usabilidade

### **3. ❓ [Sistema de FAQ](SISTEMA_FAQ.md)**
- Perguntas frequentes organizadas por categoria
- Sistema de busca inteligente
- Integração com sistema de chamados
- Redução de 50% dos chamados básicos
- Interface responsiva

### **4. 🤖 [Sistema de Inteligência Artificial](SISTEMA_IA.md)**
- Integração com OpenAI API
- Chat inteligente para pré-atendimento
- Análise automática de chamados
- Redução de 60% dos chamados via IA
- Arquitetura Java API no Oracle Cloud

### **5. 🔔 [Sistema de Notificações](SISTEMA_NOTIFICACOES.md)**
- Notificações em tempo real
- Múltiplos canais (In-App, Email, Push, SMS)
- Configurações personalizáveis
- Sistema de prioridades
- Conformidade com LGPD

### **6. 📈 [Sistema de Relatórios](SISTEMA_RELATORIOS.md)**
- Dashboard executivo
- Relatórios de performance
- Analytics e métricas
- Exportação (PDF, Excel, CSV)
- Agendamento automático

### **7. 🏗️ [Arquitetura do Sistema](ARQUITETURA_SISTEMA.md)**
- Arquitetura multiplataforma (Web, Mobile, Desktop)
- Azure Cloud: Banco de dados SQL
- Oracle Cloud: WebApp ASP.NET Core e Java API
- Aplicativos nativos: Android (Java) e Desktop (C#)
- Integração centralizada com IA via Java API
- Segurança e autenticação
- Deploy e DevOps

### **8. 📊 [Diagrama de Componentes](DIAGRAMA_COMPONENTES.md)**
- Visualização da arquitetura de componentes
- Azure Cloud: Azure SQL Database
- Oracle Cloud: HelpFast WebApp e Java API
- Aplicativos nativos: Mobile (Java Android) e Desktop (C#)
- Fluxo de comunicação entre componentes
- Integrações externas (OpenAI API)

### **9. 🔌 [Java API - Contrato Swagger](JAVA_API_SWAGGER.md)**
- Documentação completa da API Java
- Endpoints para processamento de IA
- Contrato de integração com OpenAI
- Especificações de notificações por email
- Modelos de dados e exemplos de uso

### **10. 🗄️ [Estrutura de Dados e Banco](ESTRUTURA_DADOS_BANCO.md)**
- Esquema completo do banco de dados
- Tabelas, relacionamentos e índices
- Scripts de criação e manutenção
- Políticas de segurança e conformidade LGPD
- Procedures e views para performance

### **11. 💼 [Requisitos de Negócio](REQUISITOS_NEGOCIO.md)**
- Objetivos estratégicos
- Métricas de sucesso (KPIs)
- Análise de ROI
- Conformidade LGPD
- Plano de implementação

### **12. 🧪 [Teste de Login](TESTE_LOGIN.md)**
- Cenários de teste para autenticação
- Dados de teste disponíveis
- Validações de segurança
- Como testar o sistema

## 🚀 **Quick Start**

### **1. Fazer Login**
- **Email:** admin@helpfast.com
- **Senha:** 123456

### **2. Navegar pelo Sistema**
- **Dashboard:** Funcionalidades por tipo de usuário
- **Novo Chamado:** Criar solicitação de suporte
- **Meus Chamados:** Acompanhar status
- **Relatórios:** Visualizar métricas (Admin)

## 🛠️ **Tecnologias Utilizadas**

### **☁️ Azure Cloud (Free Tier)**
- **Azure SQL Database:** Banco de dados relacional para armazenar todas as informações do sistema

### **☁️ Oracle Cloud (Free Tier)**
- **HelpFast WebApp:** Serviço de aplicação ASP.NET Core para versão web
- **Java API:** API dedicada para integração com IA, centralizando a lógica do chatBot
- **Oracle Cloud Infrastructure:** Hospedagem dos serviços web

### **📱 HelpFast Mobile App (Nativo)**
- **Java Android:** Aplicativo mobile nativo
- **Android SDK:** Desenvolvimento nativo
- **Material Design:** Interface do usuário
- **SQLite:** Banco local para cache offline

### **🖥️ HelpFast Desktop App (Nativo)**
- **C# .NET 8.0:** Aplicativo desktop nativo
- **WinForms:** Interface desktop
- **Entity Framework Core:** ORM para banco de dados
- **Material Design:** Componentes visuais

### **🔗 Integrações e APIs**
- **OpenAI API:** Inteligência artificial para chatBot
- **Java API:** Centralização da lógica de IA
- **REST APIs:** Comunicação entre componentes
- **SMTP:** Envio de notificações por email

### **🛠️ Ferramentas de Desenvolvimento**
- **GitHub:** Controle de versão
- **Visual Studio:** IDE para desenvolvimento .NET
- **Android Studio:** IDE para desenvolvimento Android
- **Docker:** Containerização

## 📞 **Suporte e Contato**

### **Desenvolvimento**
- **Desenvolvedor:** José Henrique Paiva
- **Desenvolvedor:** Gabriel Birolli Correa
- **Desenvolvedor:** Lorena Miyuki Oliveira Suzuki
- **Desenvolvedor:** Matheus Bonfim Pezzotti
- **Desenvolvedor:** Murilo Augusto E F Vieira
- **Desenvolvedor:** Pedro Henrique Pavani
- **Projeto:** UNIP ADS PIM - Suporte IA
- **Versão:** 1.0.0

### **Documentação**
- **Última Atualização:** Setembro 2025
- **Versão da Documentação:** 1.0.0
- **Idioma:** Português (Brasil)

## 📝 **Notas Importantes**

### **⚠️ Avisos**
- Sistema em desenvolvimento ativo
- Algumas funcionalidades podem estar incompletas
- Testes recomendados antes do uso em produção

### **🔒 Segurança**
- Senhas não estão criptografadas (desenvolvimento)
- Implementar hash BCrypt em produção
- Configurar HTTPS em ambiente de produção


---

**HelpFast** - Sistema Inteligente de Gerenciamento de Chamados  
*Transformando o suporte técnico através de automação e inteligência artificial*