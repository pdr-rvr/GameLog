# 🎮 GameLog - Frontend

Este repositório contém o **frontend** do projeto **GameLog**, desenvolvido como parte da disciplina de **Engenharia de Software** da turma **CP700TIN3** na instituição **FACENS**.

---

## 🏗️ Arquitetura

### Frontend com React (inspirado na Clean Architecture)

O projeto frontend segue uma estrutura **modular e organizada por responsabilidades**, inspirada nos princípios da **Clean Architecture**, adaptada ao contexto de aplicações React.

---

## ⚙️ Princípios SOLID

| Princípio                    | Implementação |
|-----------------------------|---------------|
| **Single Responsibility (SRP)** | As páginas como `TelaCadastro` e `TelaHome` possuem responsabilidades claras. Ex: o componente `FormAvaliacao` lida apenas com o formulário, e as `actions` estão separadas da UI. |
| **Open/Closed (OCP)**         | Componentes como `Navbar`, `FormAvaliacao` e `AvaliacaoCard` são reutilizáveis e permitem extensão via `props`, sem a necessidade de alterar seu código original. |
| **Liskov Substitution (LSP)** | Componentes seguem contratos claros via `props` e funcionam de forma substituível. Ex: `Navbar` recebe `activeTab`, `setActiveTab` e `usuario`, e opera conforme o esperado. |
| **Interface Segregation (ISP)** | As `props` são bem definidas e específicas, sem sobrecarga. Ex: `FormAvaliacao` recebe apenas o necessário (`jogos`, `onSubmit`, `onCancel`, etc). |
| **Dependency Inversion (DIP)** | Lógica de negócios (como cadastro e avaliações) está abstraída em arquivos de `actions`, desacoplando a UI da implementação dos serviços e facilitando testes. |

---

## ⚙️ Tecnologias Usadas

| Tecnologia       | Função no Projeto                              |
|------------------|------------------------------------------------|
| **React**        | Framework principal para construção da UI      |
| **React Router** | Navegação entre páginas                        |
| **Axios**        | Requisições HTTP para backend                  |
| **JWT (jwt-decode)** | Decodificação de tokens de autenticação  |
| **CSS Modularizado** | Estilização específica por página         |

---

## 📦 Camadas e Responsabilidades

### 🧱 Presentation Layer — Componentes e Páginas
Contém toda a interface com o usuário e lógica de interação.

- `components/`: Componentes reutilizáveis e isolados (ex: `Navbar`, `FormAvaliacao`, `Background`)
- `pages/`: Cada funcionalidade tem sua própria página com estado e lógica de exibição.
- Estilização modular com arquivos `.css` por tela, mantendo coesão visual.

> ✏️ Esta camada segue o **SRP (Single Responsibility Principle)** — cada componente/página possui uma função específica.

---

### 🔁 Application Layer — Ações e Fluxos

Responsável pela coordenação entre interface e dados externos.

- `pages/[Tela]/actions/`: Funções específicas para chamadas à API (`cadastrarUsuario`, `buscarAvaliacoes`, etc).
- Centraliza a lógica de cada caso de uso, separando da apresentação.

> ✅ Essa estrutura respeita o **Open/Closed Principle**, facilitando manutenção e extensibilidade.

---

### 🌐 Infrastructure Layer — Serviços e APIs (Axios)

Responsável por comunicação com serviços externos (backend via HTTP).

- Pode incluir uma instância Axios configurada globalmente.
- Possível extensão para interceptadores, headers dinâmicos e tratamento de erros.

> 🔒 Segue o **Dependency Inversion Principle (DIP)**: a camada de aplicação depende de abstrações (como funções de serviço), não de implementações diretas.

---

### 🔐 Auth & Sessão

- Autenticação baseada em **JWT** armazenado no `localStorage`.
- Páginas como `TelaHome` validam o token na montagem do componente, redirecionando para login caso inválido ou ausente.
- Uso de `jwt-decode` para obter os dados do usuário a partir do token.

---

## ✅ Benefícios da Arquitetura

- **Modularidade**: Código desacoplado e fácil de navegar.
- **Reusabilidade**: Componentes genéricos e reutilizáveis.
- **Escalabilidade**: Novas funcionalidades podem ser adicionadas sem impacto estrutural.
- **Manutenibilidade**: Separação clara entre UI, lógica de negócio e integração com serviços externos.


## 🚀 Como Executar o Frontend

### ✅ Pré-requisitos

- Node.js (versão 18.x ou superior)  
- npm (gerenciador de pacotes do Node.js)  
- Backend do GameLog rodando (`https://localhost:7096` ou outra URL configurada)  

---

### 🔧 Configuração Inicial

1. Clone este repositório e acesse a pasta do frontend:
git clone https://github.com/GameLogger/GameLog-FrontEnd.git
cd GameLog-FrontEnd/gamelog-frontend

2. Instale as dependências do projeto:
npm install

3. Crie um arquivo .env na raiz da pasta gamelog-frontend com o seguinte conteúdo:
REACT_APP_API_URL=http://localhost:7096

4. Inicie o servidor de desenvolvimento:
npm start

O navegador abrirá automaticamente em:
http://localhost:3000

Caso isso não aconteça, acesse manualmente esse endereço.

