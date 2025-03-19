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

## Endpoints

### Tickets

#### **GET** tickets
  **Path:** `/tickets`
  
  Fetches all tickets that are assigned for a worker account. Assigned tickets are based on the company associated to user and categories that are assigned
  
  __Context__
  >
  >**requesterEmail**
  >
  >The email of the account that requests the ticket list, provided by context.Session which is set upon login
  >
  >**Type:** String
  
  ### Response
  >
  >**Type:** array
  >
  >array of ticket object
  >
  >**array content:**
  >>
  >>**Tickets**
  >>
  >> An object containing the ticket data
  >>
  >> **type** Object See: [TicketRecord](#ticketrecord)
      
  
        
### Messages

#### Post messages
path: `/messages`

Creates a new message in the database for a specific ticket

#### Context

* **UserEmail**

  the email of the account that creates the message, taken from sessiondata that is generated on login or on fetch of a ticket via token

  **type:** string 

#### Requestbody

* **Title**

  the title of the message

  **Type:** string

* **Description**

  The message text
  
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

### Workers

##### POST
path: `/workers`

Creates a new workers in the database

##### Requestbody

* **Email**

  the email of the user that the admin wrote

  **Type:** string

##### Context

* **Company id**

  Which company to add the user to
  
  **type:** int


##### response
string message
a message stating success or failure

##### PUT
path: `/workers`

Update the workers active status to false

##### Requestbody

* **Email**

  the email that the admin clicked "remove"

  **Type:** string

##### response
string message
a message stating success or failure

##### GET
path: `/workers`

Collect all the workers from the database thats active from the admins company

##### Context

* **customerSupportEmails**

  Collect the customerSupportEmails from the database
  
  **type:** string


##### response
string message
a message stating success or failure if success **customerSupportEmails** will be return as a list

##### POST
path: `/workers/password`

Post a reset password request to the newly added user through email and store the reset token

##### Context

* **CompanyId**

  gets the company id from requestbody that the user will be added to

  **Type:** int

##### response
string message
a message stating success or failure

##### PUT
path: `/workers/password`

change the password from the link using the reset tokens from the url

##### Requestbody

* **Password**

  the password the user put in

  **Type:** string

* **Token**

  the reset token from the params in the url

  **Type:** string

##### response


string message
a message stating success or failure

### Ticket-Categories

#### Post ticket-categories

path: `/ticket-categories`

The name of the category to be created 

##### Request body

*  **Name**

  The name of the category

  **Type:** string
  
##### Response

**Message**

A message stating whether or not the category was successfully added or not

type: string

#### Get ticket-categories

path: `/ticket-categories`

Fetches all ticket categories

##### Context

**Type:** array

**array content:**

  * id

**Type:** int

  * name

**Type:** string

  * companyId

**Type:** int 

##### Response
A succsess response and a fail to connect to server response




## Objects

### TicketRecord
>
>the TicketRecord contains ticket data
>
>**Type:** object
>
>**Object variables**:
>>
>>* TicketId
>>          
>>  Id of the ticket
>>     
>>  **type:** int32
>>* Title
>>   
>>  The title of the ticket
>>     
>>   **type:** string
>>* Status
>>    
>>   the status of the ticket, enum status in database
>>   
>>   **type:** string
>>* Category
>>  
>>   the main category of the ticket
>>   
>>   **type:** string
>>* Subcategory
>>  
>>   the subcategory of the ticket
>>   
>>   **type:** string
>>* TimePosted
>>  
>>   the time the ticket was posted
>>   
>>   **type:** DateTime
>>* TimeClosed
>>  
>>   the time the ticket was closed, null if ticket is open
>>   
>>   **type:** nullable DateTime
>>* UserEmail
>>  
>>   The email of the ticket creator (not used as it was changed to user_id which is an int foreign key to user, email used to be pk for users)
>>   
>>   **type:** string
>>* CompanyFk
>>  
>>   the foreign key to company
>>   
>>   **type:** int32
>>* Elevated
>>
>>   a bool representing wether a ticket is elevated to human response
>>   
>>   **type:** bool
