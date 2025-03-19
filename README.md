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


#### Feedback 

This api inserts the  feedback data (rating, comment) into the database.

##### POST 

This post will save the review data to the database

####  Tickets

the root of the api is http://localhost:5000/api/tickets

The api is for handling everything relating to tickets

##### GET 

Gets a list of all tickets for a specific customer service worker via their email in session data

##### POST 

Creates a new ticket when provided with a category, subcategory, title, email, message and company id. Then sends a link to the ticket to the email that was provided as input.

##### PUT

Closes a ticket when provided with an id and sends an email with link to review the customer service to the email that created the ticket.


####  Tickets/resolved

##### GET 

Gets a list of all tickets for a specific customer service worker via their email in session data where status is resolved

####  Tickets/{id}

##### GET 

Gets all ticket info as well as associated messages for a specific ticket based on id

####  customer/Tickets/token

##### GET 

Gets all ticket info as well as associated messages for a specific ticket based on token that was generated and sent via email to creator of ticket


### API 

the root of the api is http://localhost:5000/api/messages and this route will upload the messages to the database for storage.

####  Messages

##### POST

This inserts the message (“title” and “description”) into the database with a reference to the ticket id (foreign key).


### API 

the root of the api is http://localhost:5000/api/workers/password and this route is meant for updating the password.

#### Workers/password

##### POST

This post will send out an email to the newly created account with a reset token in the url params and store the reset token in the database.

##### PUT

This put will update the password. We use the token in the url to change the password in the database and to find which user it is.

### API

the root of the api is http://localhost:5000/api/workers and this route is meant for creating new accounts and updating their active status in the database and as well as getting all the active workers.

#### Workers


##### POST

This post will create a new customer support

##### PUT

This put will update the users status to false meaning their account is inactive

##### GET

This get will collect all customer support that are active


### API

the root of the API is http://localhost:5000/api/ticket-categories

This API allows companies to manage categories for support tickets

#### Ticket-Categories

##### GET

Fetches all ticket categories for the logged-in user's company

##### POST

Creates a new ticket category


### API

the root of the API is http://localhost:5000/api/assign-tickets

This API allows admins to assign ticket categories to customer support workers

#### Assign-Tickets

##### GET

Fetches all assigned categories for the the logged-in user's company

##### POST

Assigns ticket categories to customer support workers

