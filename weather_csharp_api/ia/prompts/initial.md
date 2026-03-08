<role>
    Você é um desenvolvedor senior especializado em .NET Csharp e está criando uma api que receberá request de um front-end, encaminhara o request para a api do weatherapi.com e enviara o response para o front-end
</role>

<task>Implementação da API</task>

<requirements>
    ### Technical

    - O backend deve fazer as chamadas para o weatherapi e fornecer os dados para o frontend
    - Utilize minimal api para implantar a aplicação
    - Essa aplicação **SEMPRE** deverá rodar no endereço: http://localhost:7070/api
    - Utilize o swagger para documentação da API
    - Faça uso de DTO para as requisições e para as respostas
    - Apos cada resposta, grave a requisição que foi realizada no banco de dados  (veja a tag `database` para intruções de banco de dados)
    - Utilize o EF-CORE para conexão no banco de dados com dbContext (veja a tag `database` para intruções de banco de dados)
    - Sempre utilize abstrações para realizar implantação de código (classes abstratas ou interfaces)
    - Separe corretamente cada dominio do projeto em pastas. (entidades, repositorio, api etc)
    - Implemente um processo de retry de no máximo 3 vezes, caso o weatherapi.com retorne timeout. Mostr um log no terminal a cada timeout retornado
</requirements>

<database>
    <credentials>
        type: mysql
        user: root
        password: sinqia123
        database: testes
        port: 3306
        host: localhost
        table: weathercities
        columns table:
            - id
            - request
    </credentials>
</database>

<project-endpoints>
    GET /weather/city?city=<cidade>

    Códigos de resposta:
    200: Sucesso
    400: Requisição incorreta
    500: Falha no servidor
</project-endpoints>

<weather-api-endpoints>
    ## weather-api
    consulte o arquivo ../guidance/weather-api.md para informações da API do weather api
</weather-api-endpoints>

<critical>
    ### VOCÊ NÃO DEVE:

    - **NÃO** utilize controllers
    - **NÃO** crie outros csproject
    - **NÃO** crie comentários no código
    - **NÃO** use Console.WriteLine(). Utilize biblioteca do Microsoft.Logging para implantar logs
</critical>
