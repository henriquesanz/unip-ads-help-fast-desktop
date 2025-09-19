# Sistema de Perfis de Usuários - HELP FAST

## 📋 **Visão Geral**

O sistema implementa três tipos de usuários com diferentes níveis de acesso e funcionalidades:

### 🔵 **1. CLIENTE**
- **Cadastro:** Auto-cadastro (qualquer pessoa pode se cadastrar)
- **Funcionalidades:**
  - Abrir novos chamados
  - Visualizar apenas seus próprios chamados
  - Acompanhar status dos chamados
- **Restrições:** Não pode gerenciar outros usuários

### 🟡 **2. TÉCNICO**
- **Cadastro:** Apenas por Administradores
- **Funcionalidades:**
  - Visualizar todos os chamados do sistema
  - Atribuir chamados para si mesmo
  - Resolver chamados
  - Atualizar status dos chamados
- **Restrições:** Não pode gerenciar usuários

### 🔴 **3. ADMINISTRADOR**
- **Cadastro:** Apenas por outros Administradores
- **Funcionalidades:**
  - Todas as funcionalidades de Técnico
  - Gerenciar usuários (criar, editar, desativar)
  - Cadastrar Técnicos e outros Administradores
  - Acessar relatórios do sistema
  - Configurações gerais do sistema
- **Restrições:** Nenhuma (acesso total)

## 🏗️ **Arquitetura Técnica**

### **Modelos de Dados:**
- `UserRole` (Enum): Define os tipos de usuário
- `Usuario`: Modelo principal com propriedades específicas por tipo
- Relacionamento auto-referencial para rastrear quem criou cada usuário

### **Serviços:**
- `IUsuarioService`: Interface com métodos específicos por tipo
- `UsuarioService`: Implementação com validações de permissão
- Métodos específicos: `CriarClienteAsync`, `CriarTecnicoAsync`, `CriarAdministradorAsync`

### **Validações de Segurança:**
- Verificação de permissões antes de criar usuários
- Controle de acesso baseado em roles
- Rastreamento de quem criou cada usuário

## 🎯 **Regras de Negócio**

### **Hierarquia de Perfis:**
- **Usuário (Cliente):** Nível básico - apenas seus próprios chamados
- **Técnico:** Nível intermediário - chamados atribuídos + visualização geral
- **Administrador:** Nível máximo - acesso total ao sistema

### **Cadastro de Usuários:**
1. **Clientes:** Qualquer pessoa pode se auto-cadastrar
2. **Técnicos:** Apenas Administradores podem cadastrar
3. **Administradores:** Apenas Administradores podem cadastrar

### **Acesso ao Sistema:**
- **Login único:** Todos os tipos usam a mesma tela de login
- **Dashboard personalizado:** Cada tipo vê botões específicos
- **Navegação controlada:** Acesso apenas às funcionalidades permitidas

### **Usuário Master:**
- **Email:** admin@helpfast.com
- **Senha:** 123456
- **Tipo:** Administrador
- **Criado por:** Ninguém (usuário master do sistema)

## 🖥️ **Interface do Usuário**

### **Dashboard Personalizado:**
- **Cliente:** "Solicitar novo Chamado", "Meus Chamados"
- **Técnico:** "Ver Todos os Chamados", "Meus Chamados Atribuídos"
- **Administrador:** "Gerenciar Usuários", "Ver Todos os Chamados", "Relatórios"

### **Informações Exibidas:**
- Nome do usuário
- Tipo de usuário
- Descrição das permissões

## 🔐 **Segurança e Validações**

### **Validações Implementadas:**
- Email único no sistema
- Validação de formato de email
- Validação de senha (mínimo 6 caracteres)
- Validação de telefone
- Verificação de permissões antes de ações

### **Controle de Acesso:**
- Métodos específicos para cada tipo de usuário
- Validação de permissões em tempo de execução
- Rastreamento de auditoria (quem criou quem)

## 🚀 **Próximos Passos Sugeridos**

1. **Implementar hash de senhas** (BCrypt)
2. **Sistema de sessões** robusto
3. **Páginas de gerenciamento** de usuários
4. **Sistema de chamados** com atribuição
5. **Relatórios** e dashboards avançados
6. **Logs de auditoria** detalhados

## 📊 **Estrutura do Banco de Dados**

```sql
Usuarios:
- Id (PK)
- Nome
- Email (UNIQUE)
- Telefone
- Senha (hash)
- TipoUsuario (enum)
- DataCriacao
- UltimoLogin
- Ativo
- CriadoPorId (FK para Usuarios.Id)
```

## 🧪 **Como Testar**

1. **Login como Admin:** admin@helpfast.com / 123456
2. **Cadastrar Cliente:** Use a página de cadastro
3. **Verificar Dashboard:** Cada tipo mostra botões diferentes
4. **Testar Permissões:** Tentar acessar funcionalidades não permitidas

## 📝 **Notas Importantes**

- O sistema está preparado para expansão futura
- Fácil adição de novos tipos de usuário
- Arquitetura escalável e manutenível
- Segurança implementada desde o início
