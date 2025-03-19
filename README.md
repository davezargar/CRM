#PROJECT CRM


## API
the root of the api is http://localhost:5000/api

### Usage

in order to use the program you would need to start the server trough either your IDE or trough your terminal while sitting in /CRM/src/server with

```
dotnet run
```

and while being in a another terminal sitting in /CRM/src/client

```
npm run dev
```

### Installation

to install this you need to set up the server and parts of the application...
create a postgres database and import the database dump from the project repo in /database/crm-dump-final.sql
create a file called ".env" in /server with the following lines containing the connectstring for your database and localhostport for vite for your machine.
```
DatabaseConnectString="Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=crm;SearchPath=public"
Localhost="http://localhost:5173/"
```



### Messages

#### Post messages
path: `/messages`

Creates a new message in the database for a specific ticket

##### Requestbody

* **Title**

  the title of the message

  **Type:** string

* **Description**

  The message text
  
  **type:** string

* **UserEmail**

  the email of the account that creates the message, taken from sessiondata that is generated on login or on fetch of a ticket via token

  **type:** string 

* **ticket_id_fk**

  The id of the ticket that the message is created for

  **type:** int32

##### response
string message
a message stating success or failure


### Feedback 

#### Post feedback
path: `feedback-form`

Creates a new feedback review in the database.

#### Requestbody
* **feedbackdata**
  Holds the feedback data
    * **object** 
        * **rating**
          
          The rating 1-5 stars

          **type:** int
        * **comment**
            
          The feedback comment.

          **type:** string

* **token**
  
  Holds the token for that specific feedback.
  
  **type:** string

