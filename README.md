# Diffs

Emulates a simple diff tool using SQLite, EF, WebApi

Before running this small application, the web.config should be edited with the file path to the SQLite db file as follows:

In order to effectively run this project, I would recommend to use Postman and try the following endpoints:

    POST (No Auth) localhost:9999/v1/diff/1/left Raw data: "any data you would like"

    POST (No Auth) localhost:9999/v1/diff/1/right Raw data: "yan atad ouy would like"

    GET (No auth) localhost:9999/v1/diff/137/

The results shouls be in the followin format: { "LeftPartSize": "23", "RightPartSize": "23", "PartsAreEqual": false, "DiffsMetadata": [ { "Offset": "0", "Length": "12" } ], "OperationSucceeded": true, "ErrorMessage": null }

The code has been designed to support large data to compare, with parallel processing.
