# Changes to make Docker to work

## Change DB

    1) make sure TCP is allowed
    2) make sure DB allows logins
    3) add account to DB
    4) give account SA privs (for testing)

## Docker Commands

   1) to build :

    ```
    docker build -t dotnet/odata .
    ```

    this will build a docker image with the tag ID of dotnet/odata no version is mentioned

    2) start image in a container :
   
    ```
    docker run -p 8080:80 -p 1433:1433 dotnet/odata
    ```

    thiis will generate a container and start the application in a docker container 
    it can then be accessed via http://localhost:8080/odata/
