# Java API - Contrato Swagger - HelpFast

## 📋 **Visão Geral**

Este documento define o contrato da API Java responsável pela integração com IA, categorização automática de chamados, atribuição automática e envio de notificações por email.

## 🔗 **Base URL**
```
https://helpfast-java-api.oraclecloud.com/api
```

## 🔐 **Autenticação**
- **Tipo:** Bearer Token (JWT)
- **Header:** `Authorization: Bearer {token}`
- **Expiração:** 8 horas

## 📚 **Endpoints da API**

### **1. Processamento de Chamados**

#### **POST /chamados/processar**
Processa um chamado com IA para categorização e atribuição automática.

**Request Body:**
```json
{
  "id": 123,
  "titulo": "Problema com impressora",
  "descricao": "A impressora não está imprimindo documentos",
  "prioridade": "Media",
  "usuarioId": 456,
  "dataCriacao": "2024-01-15T10:30:00Z"
}
```

**Response (200 OK):**
```json
{
  "chamadoId": 123,
  "categoria": "Hardware",
  "subcategoria": "Impressora",
  "tecnicoId": 789,
  "tecnicoNome": "João Silva",
  "tecnicoEmail": "joao.silva@empresa.com",
  "confianca": 0.95,
  "sugestoes": [
    "Verificar conexão USB",
    "Verificar drivers",
    "Verificar papel e tinta"
  ],
  "notificacaoEnviada": true,
  "timestamp": "2024-01-15T10:30:15Z"
}
```

**Response (400 Bad Request):**
```json
{
  "error": "Dados inválidos",
  "message": "Descrição do chamado é obrigatória",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Response (500 Internal Server Error):**
```json
{
  "error": "Erro interno do servidor",
  "message": "Falha na comunicação com OpenAI",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### **2. Chat com IA**

#### **POST /chat/mensagem**
Envia uma mensagem para o chat com IA.

**Request Body:**
```json
{
  "usuarioId": 456,
  "mensagem": "Como alterar minha senha?",
  "contexto": {
    "tipoUsuario": "Cliente",
    "historicoInteracoes": [],
    "sistemaOperacional": "Windows 10",
    "versaoSoftware": "1.0.0"
  }
}
```

**Response (200 OK):**
```json
{
  "resposta": "Para alterar sua senha, acesse o menu Configurações > Perfil > Alterar Senha. Digite sua senha atual e a nova senha desejada.",
  "categoria": "Autenticação",
  "problemaResolvido": true,
  "sugestoesFAQ": [
    {
      "id": 15,
      "titulo": "Como alterar senha",
      "url": "/faq/alterar-senha"
    }
  ],
  "escalarParaHumano": false,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### **3. Análise de Padrões**

#### **GET /analise/padroes**
Retorna análise de padrões e tendências dos chamados.

**Query Parameters:**
- `dataInicio` (string, required): Data de início no formato ISO 8601
- `dataFim` (string, required): Data de fim no formato ISO 8601
- `categoria` (string, optional): Filtrar por categoria específica

**Response (200 OK):**
```json
{
  "periodo": {
    "inicio": "2024-01-01T00:00:00Z",
    "fim": "2024-01-31T23:59:59Z"
  },
  "padroes": [
    {
      "categoria": "Hardware",
      "frequencia": 45,
      "tendencia": "crescente",
      "problemasComuns": [
        "Impressora não funciona",
        "Computador lento",
        "Problemas de rede"
      ],
      "sugestoes": [
        "Criar FAQ sobre problemas de impressora",
        "Treinar técnicos em hardware",
        "Implementar diagnóstico automático"
      ]
    }
  ],
  "insights": [
    "Aumento de 30% em problemas de hardware no último mês",
    "Problemas de impressora são 40% dos chamados de hardware",
    "Horário de pico: 9h-11h e 14h-16h"
  ],
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### **4. Notificações**

#### **POST /notificacoes/enviar**
Envia notificação por email.

**Request Body:**
```json
{
  "tipo": "novo_chamado",
  "destinatario": {
    "email": "cliente@empresa.com",
    "nome": "João Silva"
  },
  "chamado": {
    "id": 123,
    "titulo": "Problema com impressora",
    "status": "Aberto",
    "prioridade": "Media"
  },
  "tecnico": {
    "nome": "Maria Santos",
    "email": "maria.santos@empresa.com"
  }
}
```

**Response (200 OK):**
```json
{
  "notificacaoId": "notif_123456",
  "status": "enviada",
  "destinatario": "cliente@empresa.com",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### **5. Categorização Manual**

#### **POST /categorizacao/validar**
Valida ou corrige a categorização automática de um chamado.

**Request Body:**
```json
{
  "chamadoId": 123,
  "categoriaOriginal": "Hardware",
  "categoriaCorrigida": "Software",
  "subcategoria": "Driver",
  "tecnicoId": 789,
  "observacoes": "Problema era de driver, não hardware"
}
```

**Response (200 OK):**
```json
{
  "chamadoId": 123,
  "categoriaAtualizada": "Software",
  "subcategoria": "Driver",
  "aprendizado": true,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### **6. Health Check**

#### **GET /health**
Verifica a saúde da API e suas dependências.

**Response (200 OK):**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "dependencias": {
    "openai": {
      "status": "healthy",
      "latencia": "150ms"
    },
    "email": {
      "status": "healthy",
      "latencia": "50ms"
    },
    "database": {
      "status": "healthy",
      "latencia": "25ms"
    }
  },
  "versao": "1.0.0"
}
```

## 📊 **Códigos de Status HTTP**

| Código | Descrição |
|--------|-----------|
| 200 | Sucesso |
| 400 | Dados inválidos |
| 401 | Não autorizado |
| 403 | Acesso negado |
| 404 | Recurso não encontrado |
| 429 | Muitas requisições (Rate Limit) |
| 500 | Erro interno do servidor |
| 503 | Serviço indisponível |

## 🔄 **Rate Limiting**

- **Limite:** 1000 requisições por hora por token
- **Header de Resposta:** `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`
- **Retry-After:** Tempo em segundos para próxima tentativa

## 📝 **Modelos de Dados**

### **Chamado**
```json
{
  "id": "integer",
  "titulo": "string",
  "descricao": "string",
  "prioridade": "string (Baixa|Media|Alta|Critica)",
  "usuarioId": "integer",
  "dataCriacao": "string (ISO 8601)"
}
```

### **Contexto de Chat**
```json
{
  "tipoUsuario": "string (Cliente|Tecnico|Admin)",
  "historicoInteracoes": "array",
  "sistemaOperacional": "string",
  "versaoSoftware": "string"
}
```

### **Resposta de IA**
```json
{
  "resposta": "string",
  "categoria": "string",
  "problemaResolvido": "boolean",
  "sugestoesFAQ": "array",
  "escalarParaHumano": "boolean"
}
```

## 🛡️ **Segurança**

### **Headers Obrigatórios**
```
Authorization: Bearer {jwt_token}
Content-Type: application/json
Accept: application/json
```

### **Validação de Dados**
- Todos os campos obrigatórios são validados
- Sanitização de entrada para prevenir XSS
- Validação de formato de email
- Limite de tamanho para campos de texto

### **Logs de Auditoria**
- Todas as requisições são logadas
- Informações sensíveis são mascaradas
- Logs incluem timestamp, IP, usuário e ação

## 📈 **Monitoramento**

### **Métricas Disponíveis**
- Número de requisições por minuto
- Latência média de resposta
- Taxa de erro por endpoint
- Uso de tokens OpenAI
- Emails enviados com sucesso

### **Alertas**
- Latência > 5 segundos
- Taxa de erro > 5%
- Falha na comunicação com OpenAI
- Falha no envio de emails

## 🔧 **Configuração**

### **Variáveis de Ambiente**
```
OPENAI_API_KEY=sk-...
OPENAI_MODEL=gpt-4
OPENAI_TEMPERATURE=0.7
OPENAI_MAX_TOKENS=1000
EMAIL_SMTP_HOST=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_USERNAME=helpfast@empresa.com
EMAIL_PASSWORD=app-password
DATABASE_URL=jdbc:sqlserver://...
JWT_SECRET=your-secret-key
```

## 📚 **Exemplos de Uso**

### **cURL - Processar Chamado**
```bash
curl -X POST "https://helpfast-java-api.oraclecloud.com/api/chamados/processar" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{
    "id": 123,
    "titulo": "Problema com impressora",
    "descricao": "A impressora não está imprimindo documentos",
    "prioridade": "Media",
    "usuarioId": 456,
    "dataCriacao": "2024-01-15T10:30:00Z"
  }'
```

### **cURL - Chat com IA**
```bash
curl -X POST "https://helpfast-java-api.oraclecloud.com/api/chat/mensagem" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{
    "usuarioId": 456,
    "mensagem": "Como alterar minha senha?",
    "contexto": {
      "tipoUsuario": "Cliente",
      "historicoInteracoes": [],
      "sistemaOperacional": "Windows 10",
      "versaoSoftware": "1.0.0"
    }
  }'
```

## 📝 **Notas Importantes**

- **Versionamento:** API versionada via header `API-Version`
- **Deprecação:** Endpoints deprecados terão aviso de 6 meses
- **Backward Compatibility:** Mantida por pelo menos 2 versões
- **Documentação:** Swagger UI disponível em `/swagger-ui.html`
- **Suporte:** Documentação completa e exemplos disponíveis
