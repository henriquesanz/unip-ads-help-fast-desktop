# Fluxograma de Usuários - HelpFast

## 📋 **Visão Geral**

Este documento apresenta os fluxogramas detalhados do sistema HelpFast, segregando as funcionalidades por tipo de usuário (Cliente, Técnico e Administrador) de acordo com os requisitos funcionais mapeados.

## 🔐 **Fluxo de Autenticação (Comum a Todos os Usuários)**

### **Login e Autenticação**
```
[Usuário acessa o sistema] 
    ↓
[Página de Login]
    ↓
[Inserir Email e Senha]
    ↓
[Sistema valida credenciais] → [Credenciais inválidas] → [Exibir erro] → [Retornar ao login]
    ↓
[Credenciais válidas]
    ↓
[Sistema identifica tipo de usuário]
    ↓
[Redirecionar para dashboard específico]
    ├── [Cliente] → Dashboard Cliente
    ├── [Técnico] → Dashboard Técnico
    └── [Admin] → Dashboard Administrador
```

## 👤 **Fluxo do Cliente**

### **1. Dashboard Principal do Cliente**
```
[Dashboard Cliente]
    ├── [Abrir Novo Chamado] → Fluxo de Abertura de Chamado
    ├── [Consultar FAQ] → Fluxo de Consulta FAQ
    ├── [Histórico de Chamados] → Fluxo de Histórico
    ├── [Chat com IA] → Fluxo de Pré-atendimento IA
    └── [Notificações] → Visualizar Notificações
```

### **2. Fluxo de Abertura de Chamado (RU001)**
```
[Abrir Novo Chamado]
    ↓
[Formulário de Chamado]
    ├── [Título do Problema]
    ├── [Descrição Detalhada]
    ├── [Prioridade] (Baixa, Média, Alta, Urgente)
    ├── [Categoria] (Sistema sugere via IA)
    └── [Anexos] (Opcional)
    ↓
[Enviar Chamado]
    ↓
[Sistema processa chamado]
    ├── [IA categoriza automaticamente] (RS005)
    ├── [Sistema atribui técnico] (RS006)
    └── [Notificação enviada] (RU005)
    ↓
[Chamado criado com sucesso]
    ↓
[Exibir ID do chamado e status]
    ↓
[Retornar ao Dashboard]
```

### **3. Fluxo de Consulta FAQ (RU002)**
```
[Consultar FAQ]
    ↓
[Página de FAQ]
    ├── [Buscar por palavra-chave]
    ├── [Navegar por categorias]
    └── [Visualizar perguntas frequentes]
    ↓
[Selecionar pergunta]
    ↓
[Visualizar resposta detalhada]
    ├── [Resposta resolve problema] → [Marcar como útil] → [Fechar FAQ]
    └── [Resposta não resolve] → [Abrir chamado] → Fluxo de Abertura de Chamado
```

### **4. Fluxo de Histórico de Chamados (RU004)**
```
[Histórico de Chamados]
    ↓
[Lista de chamados do usuário]
    ├── [Filtrar por status]
    ├── [Filtrar por data]
    └── [Buscar por ID]
    ↓
[Selecionar chamado]
    ↓
[Detalhes do chamado]
    ├── [ID, Data, Status]
    ├── [Histórico de alterações]
    ├── [Comentários do técnico]
    └── [Anexos]
    ↓
[Voltar ao histórico]
```

### **5. Fluxo de Pré-atendimento com IA (RU003)**
```
[Chat com IA]
    ↓
[Interface de chat]
    ↓
[Cliente descreve problema]
    ↓
[IA analisa e responde]
    ├── [IA resolve problema] → [Solução apresentada] → [Cliente confirma resolução] → [Chamado fechado]
    └── [IA não resolve] → [Sugerir abertura de chamado] → [Cliente confirma] → Fluxo de Abertura de Chamado
```

### **6. Fluxo de Notificações (RU005)**
```
[Notificação recebida]
    ↓
[Exibir notificação]
    ├── [Notificação in-app]
    ├── [Email]
    └── [Push notification]
    ↓
[Cliente visualiza notificação]
    ├── [Marcar como lida]
    └── [Acessar chamado relacionado] → Detalhes do chamado
```

## 🔧 **Fluxo do Técnico**

### **1. Dashboard Principal do Técnico**
```
[Dashboard Técnico]
    ├── [Chamados Atribuídos] → Fluxo de Atendimento
    ├── [Todos os Chamados] → Visualizar todos os chamados
    ├── [Relatórios] → Relatórios de performance
    └── [Notificações] → Notificações do técnico
```

### **2. Fluxo de Visualização de Chamados (RU006)**
```
[Chamados Atribuídos]
    ↓
[Lista de chamados do técnico]
    ├── [Filtrar por prioridade]
    ├── [Filtrar por status]
    └── [Ordenar por data]
    ↓
[Selecionar chamado]
    ↓
[Detalhes do chamado]
    ├── [Informações do cliente]
    ├── [Descrição do problema]
    ├── [Histórico de alterações]
    └── [Anexos]
    ↓
[Ações disponíveis]
    ├── [Alterar Status] → Fluxo de Alteração de Status
    ├── [Adicionar Comentário]
    └── [Escalar para Admin]
```

### **3. Fluxo de Alteração de Status (RU007)**
```
[Alterar Status do Chamado]
    ↓
[Selecionar novo status]
    ├── [Em Andamento]
    ├── [Aguardando Cliente]
    ├── [Resolvido]
    └── [Fechado]
    ↓
[Adicionar comentário] (Opcional)
    ↓
[Confirmar alteração]
    ↓
[Sistema atualiza status]
    ├── [Registra alteração no log] (RS004)
    ├── [Envia notificação ao cliente] (RU005)
    └── [Atualiza dashboard em tempo real] (RNF001)
    ↓
[Status alterado com sucesso]
    ↓
[Retornar ao chamado]
```

### **4. Fluxo de Atendimento Completo**
```
[Iniciar Atendimento]
    ↓
[Ler descrição do problema]
    ↓
[Analisar anexos e histórico]
    ↓
[Definir estratégia de resolução]
    ↓
[Alterar status para "Em Andamento"]
    ↓
[Implementar solução]
    ├── [Solução encontrada] → [Alterar para "Resolvido"] → [Aguardar confirmação do cliente]
    └── [Precisa de mais informações] → [Alterar para "Aguardando Cliente"] → [Solicitar informações]
    ↓
[Cliente confirma resolução]
    ↓
[Fechar chamado]
    ↓
[Registrar solução no FAQ] (Opcional)
```

## 👨‍💼 **Fluxo do Administrador**

### **1. Dashboard Principal do Administrador**
```
[Dashboard Administrador]
    ├── [Gerenciar Chamados] → Fluxo de Gerenciamento de Chamados
    ├── [Gerenciar Usuários] → Fluxo de Gerenciamento de Usuários
    ├── [Relatórios Executivos] → Relatórios e métricas
    ├── [Configurações do Sistema] → Configurações gerais
    └── [Logs de Auditoria] → Visualizar logs do sistema
```

### **2. Fluxo de Gerenciamento de Chamados (RU008)**
```
[Gerenciar Chamados]
    ↓
[Painel de todos os chamados]
    ├── [Filtrar por status]
    ├── [Filtrar por técnico]
    ├── [Filtrar por prioridade]
    └── [Filtrar por data]
    ↓
[Selecionar chamado]
    ↓
[Ações administrativas]
    ├── [Alterar Status] → Fluxo de Alteração de Status Admin
    ├── [Atribuir Técnico] → Fluxo de Atribuição
    ├── [Reclassificar Prioridade]
    ├── [Adicionar Comentário]
    └── [Escalar para outro Admin]
```

### **3. Fluxo de Alteração de Status Admin (RU009)**
```
[Alterar Status (Admin)]
    ↓
[Selecionar novo status]
    ├── [Aberto]
    ├── [Em Andamento]
    ├── [Aguardando Cliente]
    ├── [Resolvido]
    ├── [Fechado]
    └── [Cancelado]
    ↓
[Adicionar justificativa] (Obrigatório)
    ↓
[Confirmar alteração]
    ↓
[Sistema processa alteração]
    ├── [Registra no log de auditoria] (RS004)
    ├── [Notifica cliente e técnico] (RU005)
    └── [Atualiza dashboards] (RNF001)
    ↓
[Alteração realizada com sucesso]
```

### **4. Fluxo de Atribuição de Chamados (RU011)**
```
[Atribuir Chamado]
    ↓
[Selecionar chamado]
    ↓
[Escolher técnico]
    ├── [Lista de técnicos disponíveis]
    ├── [Filtrar por especialidade]
    └── [Ver carga de trabalho]
    ↓
[Confirmar atribuição]
    ↓
[Sistema processa atribuição]
    ├── [Atualiza chamado]
    ├── [Notifica técnico] (RU005)
    └── [Registra no log] (RS004)
    ↓
[Atribuição realizada]
```

### **5. Fluxo de Gerenciamento de Usuários (RU010)**
```
[Gerenciar Usuários]
    ↓
[Lista de usuários]
    ├── [Filtrar por tipo]
    ├── [Filtrar por status]
    └── [Buscar por nome/email]
    ↓
[Ações disponíveis]
    ├── [Adicionar Usuário] → Fluxo de Cadastro
    ├── [Editar Usuário] → Fluxo de Edição
    ├── [Desativar Usuário]
    └── [Alterar Permissões]
```

### **6. Fluxo de Cadastro de Usuário**
```
[Adicionar Usuário]
    ↓
[Formulário de cadastro]
    ├── [Nome completo]
    ├── [Email]
    ├── [Telefone]
    ├── [Tipo de usuário] (Cliente, Técnico, Admin)
    └── [Permissões específicas]
    ↓
[Validar dados]
    ↓
[Criar usuário]
    ↓
[Sistema processa cadastro]
    ├── [Cria conta]
    ├── [Envia credenciais por email]
    └── [Registra no log] (RS004)
    ↓
[Usuário criado com sucesso]
```

### **7. Fluxo de Hierarquia de Perfis (RU012)**
```
[Sistema de Hierarquia]
    ↓
[Definir permissões por perfil]
    ├── [Cliente]
    │   ├── [Criar chamados]
    │   ├── [Visualizar próprios chamados]
    │   ├── [Consultar FAQ]
    │   └── [Chat com IA]
    ├── [Técnico]
    │   ├── [Visualizar chamados atribuídos]
    │   ├── [Alterar status dos chamados]
    │   ├── [Adicionar comentários]
    │   └── [Acessar relatórios básicos]
    └── [Administrador]
        ├── [Gerenciar todos os chamados]
        ├── [Gerenciar usuários]
        ├── [Acessar todos os relatórios]
        ├── [Configurar sistema]
        └── [Visualizar logs de auditoria]
    ↓
[Aplicar permissões]
    ↓
[Validar acesso por perfil]
```

## 🔄 **Fluxos de Sistema (Automação)**

### **1. Fluxo de Categorização Automática (RS005)**
```
[Novo chamado criado]
    ↓
[IA analisa conteúdo]
    ├── [Título do chamado]
    ├── [Descrição do problema]
    └── [Palavras-chave]
    ↓
[IA classifica categoria]
    ├── [Hardware]
    ├── [Software]
    ├── [Rede]
    ├── [Email]
    └── [Outros]
    ↓
[Atualizar chamado com categoria]
    ↓
[Continuar para atribuição]
```

### **2. Fluxo de Atribuição Automática (RS006)**
```
[Chamado categorizado]
    ↓
[Sistema identifica técnicos disponíveis]
    ├── [Filtrar por especialidade]
    ├── [Verificar carga de trabalho]
    └── [Considerar prioridade]
    ↓
[Selecionar técnico mais adequado]
    ↓
[Atribuir chamado automaticamente]
    ↓
[Notificar técnico] (RU005)
    ↓
[Registrar no log] (RS004)
```

### **3. Fluxo de Notificações (RU005)**
```
[Evento que gera notificação]
    ├── [Chamado criado]
    ├── [Status alterado]
    ├── [Comentário adicionado]
    └── [Chamado atribuído]
    ↓
[Sistema identifica destinatários]
    ├── [Cliente do chamado]
    ├── [Técnico responsável]
    └── [Administradores] (se necessário)
    ↓
[Enviar notificações]
    ├── [Notificação in-app]
    ├── [Email]
    └── [Push notification]
    ↓
[Registrar envio no log]
```

## 📊 **Fluxo de Relatórios e Métricas**

### **1. Fluxo de Geração de Relatórios**
```
[Acessar Relatórios]
    ↓
[Selecionar tipo de relatório]
    ├── [Performance por técnico]
    ├── [Volume de chamados]
    ├── [Tempo de resolução]
    └── [Satisfação do cliente]
    ↓
[Definir período]
    ↓
[Gerar relatório]
    ↓
[Visualizar resultados]
    ├── [Gráficos]
    ├── [Tabelas]
    └── [Métricas]
    ↓
[Exportar] (PDF, Excel, CSV)
```

## 🔍 **Fluxo de Auditoria (RS004)**

### **1. Fluxo de Registro de Alterações**
```
[Qualquer alteração no sistema]
    ↓
[Sistema captura dados]
    ├── [Usuário que fez alteração]
    ├── [Data e hora]
    ├── [Tipo de operação]
    ├── [Dados antes da alteração]
    └── [Dados após alteração]
    ↓
[Registrar no log de auditoria]
    ↓
[Manter por 7 anos] (RNF012)
```

## 📱 **Fluxo Multiplataforma (RNF003)**

### **1. Acesso via Web**
```
[Navegador web]
    ↓
[URL do sistema]
    ↓
[Interface responsiva]
    ↓
[Funcionalidades completas]
```

### **2. Acesso via Mobile (Android)**
```
[Aplicativo Android]
    ↓
[Login]
    ↓
[Interface adaptada para mobile]
    ├── [Funcionalidades principais]
    ├── [Notificações push]
    └── [Sincronização em tempo real]
```

### **3. Acesso via Desktop (C#)**
```
[Aplicação Desktop]
    ↓
[Login]
    ↓
[Interface desktop completa]
    ├── [Todas as funcionalidades]
    ├── [Notificações do sistema]
    └── [Integração com sistema operacional]
```

## 🎯 **Resumo dos Fluxos por Perfil**

### **Cliente (5 fluxos principais)**
1. Abertura de chamados
2. Consulta FAQ
3. Histórico de chamados
4. Pré-atendimento com IA
5. Recebimento de notificações

### **Técnico (4 fluxos principais)**
1. Visualização de chamados atribuídos
2. Alteração de status
3. Atendimento completo
4. Consulta de relatórios

### **Administrador (7 fluxos principais)**
1. Gerenciamento de chamados
2. Alteração de status (todos os chamados)
3. Atribuição de chamados
4. Gerenciamento de usuários
5. Cadastro de usuários
6. Configuração de hierarquia
7. Acesso a logs de auditoria

### **Sistema (3 fluxos automáticos)**
1. Categorização automática via IA
2. Atribuição automática de chamados
3. Envio de notificações

---

**Nota:** Todos os fluxos respeitam os requisitos de segurança (LGPD), performance (tempo de resposta < 5s) e disponibilidade (24/7) conforme especificado nos requisitos não funcionais.
