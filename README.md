# SearchService - FastTech Foods

Serviço de busca e filtro para o sistema de fast food FastTech Foods, implementado como parte da arquitetura de microsserviços.

## 🎯 Funcionalidades

- **Busca por produtos**: Busca por nome (case-insensitive)
- **Filtros avançados**: Por faixa de preço, disponibilidade
- **Paginação**: Suporte a paginação com controle de tamanho de página
- **Ordenação**: Por nome ou preço

## 🏗️ Arquitetura

O serviço segue os princípios da **Clean Architecture**:

```
SearchService/
├── src/
│   ├── SearchService.Domain/          # Entidades e interfaces
│   ├── SearchService.Application/     # Casos de uso (CQRS + MediatR)
│   ├── SearchService.Infrastructure/  # Repositórios e integrações
│   └── SearchService.API/            # Controllers e configuração
```

## 🛠️ Tecnologias

- **.NET 8**: Framework principal
- **MongoDB**: Banco de dados NoSQL com índices otimizados
- **MediatR**: Implementação do padrão CQRS
- **Docker**: Containerização
- **Swagger**: Documentação da API

## 🚀 Como Executar

### Pré-requisitos
- .NET 8 SDK
- Docker e Docker Compose
- MongoDB (ou usar via Docker)

### Via Docker Compose (Recomendado)

```bash
cd SearchService
docker-compose up -d
```

A aplicação estará disponível em:
- **API**: http://localhost:5003
- **Swagger**: http://localhost:5003/swagger
- **MongoDB**: localhost:27017

### Via dotnet run

```bash
cd SearchService/src/SearchService.API
dotnet restore
dotnet run
```

## 📋 Endpoints da API

### 🔍 Buscar Itens do Menu
```http
GET /api/search/menu-items?name={nome}&minPrice={min}&maxPrice={max}&isAvailable={bool}&page={num}&pageSize={size}&sortBy={campo}&sortDescending={bool}
```

**Parâmetros de Query:**
- `name` (string, opcional): Nome do produto (busca parcial)
- `minPrice` (decimal, opcional): Preço mínimo
- `maxPrice` (decimal, opcional): Preço máximo
- `isAvailable` (bool, opcional): Filtrar por disponibilidade
- `page` (int, padrão=1): Número da página
- `pageSize` (int, padrão=10, máx=100): Itens por página
- `sortBy` (string, padrão="Name"): Campo para ordenação (Name, Price)
- `sortDescending` (bool, padrão=false): Ordenação decrescente

**Exemplo de Response:**
```json
{
  "items": [
    {
      "id": "507f1f77bcf86cd799439011",
      "name": "Big Burger",
      "description": "Hambúrguer artesanal com ingredientes frescos",
      "price": 25.90,
      "isAvailable": true
    }
  ],
  "totalCount": 50,
  "page": 1,
  "pageSize": 10,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### 📄 Obter Item por ID
```http
GET /api/search/menu-items/{id}
```



## 🔧 Configuração

### appsettings.json
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "FastTechFoods",
    "MenuItemsCollectionName": "MenuItems"
  }
}
```

## 📊 Otimizações

### Índices MongoDB
O serviço cria automaticamente índices otimizados:
- **Índice composto**: Name + Category + Price + IsAvailable
- **Índice de texto**: Name + Description (para busca textual)

### Funcionalidades de Performance
- Paginação obrigatória (máximo 100 itens por página)
- Uso de projeções MongoDB
- Índices otimizados para filtros comuns
- Cache de categorias

## 🔄 Integração com MenuService

O SearchService conecta diretamente na mesma base MongoDB do MenuService para buscar os dados.

## 🧪 Exemplos de Uso

### Buscar hamburgueres até R$ 30
```bash
curl "http://localhost:5003/api/search/menu-items?maxPrice=30&name=burger"
```

### Listar itens disponíveis, ordenados por preço crescente
```bash
curl "http://localhost:5003/api/search/menu-items?isAvailable=true&sortBy=price"
```

### Paginação - página 2, 5 itens por página
```bash
curl "http://localhost:5003/api/search/menu-items?page=2&pageSize=5"
```

## 🐳 Docker

### Build da imagem
```bash
docker build -t searchservice .
```

### Run com variáveis de ambiente
```bash
docker run -p 5003:80 \
  -e MongoDbSettings__ConnectionString=mongodb://host.docker.internal:27017 \
  -e MongoDbSettings__DatabaseName=FastTechFoods \
  searchservice
```

## 📈 Monitoramento

O serviço está preparado para integração com:
- **Zabbix**: Monitoramento de infraestrutura
- **Grafana**: Dashboards de performance
- **Application Insights**: Logs e métricas da aplicação

## 🔧 Desenvolvimento

### Executar testes
```bash
dotnet test
```

### Adicionar nova categoria
1. Adicione no enum/lista de categorias válidas
2. Execute a sincronização com o MenuService
3. Os índices MongoDB serão atualizados automaticamente

---

**Desenvolvido para o Hackaton FastTech Foods** 🍔🚀 