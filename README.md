# ODataBatching8

This code uses odata 8 preview and has all thew parts necessary to test

in the supplemental data folder is the SQL necessary to generate the tables and populate the data

to execute the tests you can either use Postman or curl.  

## Series of Get calls in one $batch operation

## Non-Json Package $batch call

### Non-Json curl

    ```
        curl --location --request POST 'https://localhost:44367/odata/$batch' \
        --header 'Content-Type: multipart/mixed; boundary=batch_AAA123' \
        --header 'Accept: multipart/mixed' \
        --data-raw '--batch_AAA123
        Content-Type: application/http
        Content-Transfer-Encoding: binary

        GET https://localhost:44367/odata/Books/a35be09d-004b-d2df-7ae5-0725e602aed7 HTTP/1.1
        Accept: application/json;odata.metadata=minimal
        Accept-Charset: UTF-8
        User-Agent: Microsoft ADO.NET Data Services

        --batch_AAA123
        Content-Type: application/http
        Content-Transfer-Encoding: binary

        GET https://localhost:44367/odata/Users/975ffc54-795e-c9f0-6774-0883f1643b2e HTTP/1.1
        Accept: application/json;odata.metadata=minimal
        Accept-Charset: UTF-8
        User-Agent: Microsoft ADO.NET Data Services

        --batch_AAA123--'
    ```

### Non-Json Postman

#### Headers

    ```
        Content-Type: multipart/mixed; boundary=batch_AAA123
        Accept: multipart/mixed
        ForceUseSession: true
    ```

#### Body

    ```
        --batch_AAA123
        Content-Type: application/http
        Content-Transfer-Encoding: binary

        GET https://localhost:44367/odata/Books/a35be09d-004b-d2df-7ae5-0725e602aed7 HTTP/1.1
        Accept: application/json;odata.metadata=minimal
        Accept-Charset: UTF-8
        User-Agent: Microsoft ADO.NET Data Services

        --batch_AAA123
        Content-Type: application/http
        Content-Transfer-Encoding: binary

        GET https://localhost:44367/odata/Users/975ffc54-795e-c9f0-6774-0883f1643b2e HTTP/1.1
        Accept: application/json;odata.metadata=minimal
        Accept-Charset: UTF-8
        User-Agent: Microsoft ADO.NET Data Services

        --batch_AAA123--
    ```

## Json Package batch

### Json Package curl

        ```
            curl --location --request POST 'https://localhost:44367/odata/$batch' \
            --header 'Content-Type: application/json' \
            --header 'Accept;' \
            --data-raw '{
                "requests": [
                    {
                        "id": "0",
                        "method": "get",
                        "url": "Books/a35be09d-004b-d2df-7ae5-0725e602aed7"
                    },
                    {
                        "id": "1",
                        "method": "get",
                        "url": "Users/975ffc54-795e-c9f0-6774-0883f1643b2e"
                    },
                    {
                    "id": "2",
                    "method": "get",
                    "url": "Groups/45a19125-3b1c-858e-39a6-2e5d92576714"
                }
                ]
            }'
        ```

### Json Postman

#### Json Postman Request Headers

        ```
        Content-Type : application/json
        Accept : 
        ForceUseSession : true
        ```

#### Json Postman Body

    ```
        {
            "requests": [
                {
                    "id": "0",
                    "method": "get",
                    "url": "Books/a35be09d-004b-d2df-7ae5-0725e602aed7"
                },
                {
                    "id": "1",
                    "method": "get",
                    "url": "Users/975ffc54-795e-c9f0-6774-0883f1643b2e"
                },
                {
                "id": "2",
                "method": "get",
                "url": "Groups/45a19125-3b1c-858e-39a6-2e5d92576714"
            }
            ]
        }
    ```

## Multiple Patch calls using $batch

The methods to use multiple Patch/Post/Update is the same but is a little different.  There are two changes that had to occur

1) the code had to have transactions added into the default batch handler since it is not done by default

2) The message has additional fields to mark seperate changesets.  This allow oData to bundle calls together through that changeset Id 

### Json Patch $batch call

#### Json Patch Postman $batch Request Headers

    ```        
        Content-Type: application/json
    ```

#### Json Patch Postman $batch sample body

##### note: this call will fail since the Address in the 3rd operation is not a Guid.  This is to test transactions

    ```
        {
            "requests": [
                {
                    "id": "0",
                    "atomicityGroup": "group1",
                    "method": "patch",
                    "url": "Books/a35be09d-004b-d2df-7ae5-0725e602aed7",
                    "headers": {
                        "Content-Type": "application/json;odata=verbose"
                    },
                    "body": {
                        "@odata.context": "https://localhost:44367/odata/$metadata#Books/$entity",
                        "Price": 88.11
                    }
                },
                {
                    "id": "1",
                    "atomicityGroup": "group1",
                    "method": "patch",
                    "url": "Books/a35be09d-004b-d2df-7ae5-0725e602aed7",
                    "headers": {
                        "Content-Type": "application/json;odata=verbose"
                    },
                    "body": {
                        "@odata.context": "https://localhost:44367/odata/$metadata#Books/$entity",
                        "Author": "Debbie Macomber 9"
                    }
                },
                {
                    "id": "2",
                    "atomicityGroup": "group1",
                    "method": "patch",
                    "url": "Books/a35be09d-004b-d2df-7ae5-0725e602aed7",
                    "headers": {
                        "Content-Type": "application/json;odata=verbose"
                    },
                    "body": {
                        "@odata.context": "https://localhost:44367/odata/$metadata#Books/$entity",
                        "Address": "Think and Grow Rich 2"
                    }
                }
            ]
        }
    ```
#### Json Patch Postman $batch sample body with one working transaction

```
   {
        "requests": [
            {
                "id": "0",
                "atomicityGroup": "group1",
                "method": "patch",
                "url": "Books/a35be09d-004b-d2df-7ae5-0725e602aed7",
                "headers": {
                    "Content-Type": "application/json;odata.metadata=minimal; odata.streaming=true"
                },
                "body": {
                    "@odata.context": "https://localhost:44367/odata/$metadata#Books/$entity",
                    "Price": 22.2222
                }
            },
            {
                "id": "1",
                "atomicityGroup": "group1",
                "method": "patch",
                "url": "Books/a35be09d-004b-d2df-7ae5-0725e602aed7",
                "headers": {
                    "Content-Type": "application/json;odata.metadata=minimal; odata.streaming=true"
                },
                "body": {
                    "@odata.context": "https://localhost:44367/odata/$metadata#Books/$entity",
                    "Author": "Debbie Macomber 22"
                }
            },
            {
                "id": "2",
                "atomicityGroup": "group1",
                "method": "patch",
                "url": "Books/a35be09d-004b-d2df-7ae5-0725e602aed7",
                "headers": {
                    "Content-Type": "application/json;odata.metadata=minimal; odata.streaming=true"
                },
                "body": {              
                    "@odata.context": "https://localhost:44367/odata/$metadata#Books/$entity", 
                    "Address": "Think and Grow Rich Group1"
                }
            },
            {
                "id": "3",
                "atomicityGroup": "group2",
                "method": "patch",
                "url": "Books/a35be09d-004b-d2df-7ae5-0725e602aed7",
                "headers": {
                    "Content-Type": "application/json;odata.metadata=minimal; odata.streaming=true"
                },
                "body": {              
                    "@odata.context": "https://localhost:44367/odata/$metadata#Books/$entity", 
                    "Title": "Think and Grow Rich Group2"
                }
            }
        ]
    } 
```