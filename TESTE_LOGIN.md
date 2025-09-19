# Teste de Mensagens de Login - HELP FAST

## Cenários de Teste para Validação de Login

### 1. **Login Válido (Sucesso)**
- **E-mail:** `admin@helpfast.com`
- **Senha:** `123456`
- **Resultado:** Redirecionamento para o Dashboard

### 2. **E-mail Inválido (Usuário não encontrado)**
- **E-mail:** `usuario@inexistente.com`
- **Senha:** `qualquer123`
- **Resultado:** Mensagem "Usuário não encontrado. Verifique o e-mail informado."

### 3. **Senha Incorreta**
- **E-mail:** `admin@helpfast.com`
- **Senha:** `senhaerrada`
- **Resultado:** Mensagem "Senha incorreta. Tente novamente."

### 4. **Campos Vazios**
- **E-mail:** (vazio)
- **Senha:** (vazio)
- **Resultado:** Mensagens de validação dos campos obrigatórios

### 5. **E-mail Inválido (Formato)**
- **E-mail:** `email-invalido`
- **Senha:** `qualquer123`
- **Resultado:** Mensagem de validação de formato de e-mail

### 6. **Senha Muito Curta**
- **E-mail:** `admin@helpfast.com`
- **Senha:** `123`
- **Resultado:** Mensagem "A senha deve ter pelo menos 6 caracteres"

## Dados de Teste Disponíveis

### Usuário Administrador (Pré-cadastrado)
- **E-mail:** admin@helpfast.com
- **Senha:** 123456
- **Nome:** Administrador
- **Telefone:** (11) 99999-9999

### Cadastrar Novo Usuário
- Use a página de cadastro para criar novos usuários
- Teste o login com os novos usuários criados
