# <img src="/src/icon.png" height="30px"> NServiceBus.SqlServer.Native

[![Build status](https://ci.appveyor.com/api/projects/status/vlhu32bab9gk2olc/branch/main?svg=true)](https://ci.appveyor.com/project/SimonCropp/nservicebus-sqlnative)
[![NuGet Status](https://img.shields.io/nuget/v/NServiceBus.SqlServer.Native.svg?label=SqlServer.Native)](https://www.nuget.org/packages/NServiceBus.SqlServer.Native/)
[![NuGet Status](https://img.shields.io/nuget/v/NServiceBus.SqlServer.HttpPassthrough.svg?label=SqlServer.HttpPassthrough)](https://www.nuget.org/packages/NServiceBus.SqlServer.HttpPassthrough/)
[![NuGet Status](https://img.shields.io/nuget/v/NServiceBus.SqlServer.Deduplication.svg?label=SqlServer.Deduplication)](https://www.nuget.org/packages/NServiceBus.SqlServer.Deduplication/)

SQL Server Transport Native is a shim providing low-level access to the [NServiceBus SQL Server Transport](https://docs.particular.net/transports/sql/) with no NServiceBus or SQL Server Transport reference required.

<!--- StartOpenCollectiveBackers -->

[Already a Patron? skip past this section](#endofbacking)


## Community backed

**It is expected that all developers either [become a Patron](https://opencollective.com/nservicebusextensions/contribute/patron-6976) to use NServiceBusExtensions. [Go to licensing FAQ](https://github.com/NServiceBusExtensions/Home/#licensingpatron-faq)**


### Sponsors

Support this project by [becoming a Sponsor](https://opencollective.com/nservicebusextensions/contribute/sponsor-6972). The company avatar will show up here with a website link. The avatar will also be added to all GitHub repositories under the [NServiceBusExtensions organization](https://github.com/NServiceBusExtensions).


### Patrons

Thanks to all the backing developers. Support this project by [becoming a patron](https://opencollective.com/nservicebusextensions/contribute/patron-6976).

<img src="https://opencollective.com/nservicebusextensions/tiers/patron.svg?width=890&avatarHeight=60&button=false">

<a href="#" id="endofbacking"></a>

<!--- EndOpenCollectiveBackers -->


toc


## NuGet packages

 * https://nuget.org/packages/NServiceBus.SqlServer.Native/
 * https://nuget.org/packages/NServiceBus.SqlServer.HttpPassthrough/
 * https://nuget.org/packages/NServiceBus.SqlServer.Deduplication/


## Usage scenarios

 * **Error or Audit queue handling**: Allows to consume messages from error and audit queues, for example to move them to a long-term archive. NServiceBus expects to have a queue per message type, so NServiceBus endpoints are not suitable for processing error or audit queues. SQL Native allows manipulation or consumption of queues containing multiple types of messages.
 * **Corrupted or malformed messages**: Allows to process poison messages which can't be deserialized by NServiceBus. In SQL Native message headers and body are treated as a raw string and byte array, so corrupted or malformed messages can be read and manipulated in code to correct any problems.
 * **Deployment or decommission**: Allows to perform common operational activities, similar to [operations scripts](https://docs.particular.net/transports/sql/operations-scripting#native-send-the-native-send-helper-methods-in-c). Running [installers](https://docs.particular.net/nservicebus/operations/installers) requires starting a full endpoint. This is not always ideal during the execution of a deployment or decommission. SQL Native allows creating or deleting of queues with no running endpoint, and with significantly less code. This also makes it a better candidate for usage in deployment scripting languages like PowerShell.
 * **Bulk operations**: SQL Native supports sending and receiving of multiple messages within a single `SQLConnection` and `SQLTransaction`.
 * **Explicit connection and transaction management**: NServiceBus abstracts the `SQLConnection` and `SQLTransaction` creation and management. SQL Native allows any consuming code to manage the scope and settings of both the `SQLConnection` and `SQLTransaction`.
 * **Message pass through**: SQL Native reduces the amount of boilerplate code and simplifies development.


## Main Queue


### Queue management

Queue management for the [native delayed delivery](https://docs.particular.net/transports/sql/native-delayed-delivery) functionality.

See also [SQL Server Transport - SQL statements](https://docs.particular.net/transports/sql/sql-statements#installation).


#### Create

The queue can be created using the following:

snippet: CreateQueue


#### Delete

The queue can be deleted using the following:

snippet: DeleteQueue


### Sending messages

Sending to the main transport queue.


#### Single

Sending a single message.

snippet: Send


#### Batch

Sending a batch of messages.

snippet: SendBatch


### Reading messages

"Reading" a message returns the data from the database without deleting it.


#### Single

Reading a single message.

snippet: Read


#### Batch

Reading a batch of messages.

snippet: ReadBatch


#### RowVersion tracking

For many scenarios, it is likely to be necessary to keep track of the last message `RowVersion` that was read. A lightweight implementation of the functionality is provided by `RowVersionTracker`. `RowVersionTracker` stores the current `RowVersion` in a table containing a single column and row.

snippet: RowVersionTracker

Note that this is only one possible implementation of storing the current `RowVersion`.


#### Processing loop

For scenarios where continual processing (reading and executing some code with the result) of incoming messages is required, `MessageProcessingLoop` can be used. 

An example use case is monitoring an [error queue](https://docs.particular.net/nservicebus/recoverability/configure-error-handling). Some action should be taken when a message appears in the error queue, but it should remain in that queue in case it needs to be retried. 

Note that in the below snippet, the above `RowVersionTracker` is used for tracking the current `RowVersion`.

snippet: ProcessingLoop


### Consuming messages

"Consuming" a message returns the data from the database and also deletes that message.


#### Single

Consume a single message.

snippet: Consume


#### Batch

Consuming a batch of messages.

snippet: ConsumeBatch


#### Consuming loop

For scenarios where continual consumption (consuming and executing some code with the result) of incoming messages is required, `MessageConsumingLoop` can be used.

An example use case is monitoring an [audit queue](https://docs.particular.net/nservicebus/operations/auditing). Some action should be taken when a message appears in the audit queue, and it should be purged from the queue to free up the storage space. 

snippet: ConsumeLoop


## Delayed Queue


### Queue management

Queue management for the [native delayed delivery](https://docs.particular.net/transports/sql/native-delayed-delivery) functionality.

See also [SQL Server Transport - SQL statements](https://docs.particular.net/transports/sql/sql-statements#create-delayed-queue-table).


#### Create

The queue can be created using the following:

snippet: CreateDelayedQueue


#### Delete

The queue can be deleted using the following:

snippet: DeleteDelayedQueue


### Sending messages


#### Single

Sending a single message.

snippet: SendDelayed


#### Batch

Sending a batch of messages.

snippet: SendDelayedBatch


### Reading messages

"Reading" a message returns the data from the database without deleting it.


#### Single

Reading a single message.

snippet: ReadDelayed


#### Batch

Reading a batch of messages.

snippet: ReadDelayedBatch


### Consuming messages

"Consuming" a message returns the data from the database and also deletes that message.


#### Single

Consume a single message.

snippet: ConsumeDelayed


#### Batch

Consuming a batch of messages.

snippet: ConsumeDelayedBatch


## Headers

There is a headers helpers class `NServiceBus.Transport.SqlServerNative.Headers`.

It contains several [header](https://docs.particular.net/nservicebus/messaging/headers) related utilities.


## Subscriptions

Queue management for the [native publish subscribe](https://docs.particular.net/transports/sql/native-publish-subscribe) functionality.


### Table management


#### Create

The table can be created using the following:

snippet: CreateSubscriptionTable


#### Delete

The table can be deleted using the following:

snippet: DeleteSubscriptionTable


## Deduplication

Some scenarios, such as HTTP message pass through, require message deduplication.


### Table management


#### Create

The table can be created using the following:

snippet: CreateDeduplicationTable


#### Delete

The table can be deleted using the following:

snippet: DeleteDeduplicationTable


### Sending messages

Sending to the main transport queue with deduplication.


#### Single

Sending a single message with deduplication.

snippet: SendWithDeduplication


#### Batch

Sending a batch of messages with deduplication.

snippet: SendBatchWithDeduplication


### Deduplication cleanup

Deduplication records need to live for a period of time after the initial corresponding message has been send. In this way an subsequent message, with the same message id, can be ignored. This necessitates a periodic cleanup process of deduplication records. This is achieved by using `DeduplicationCleanerJob`:

At application startup, start an instance of `DeduplicationCleanerJob`.

snippet: DeduplicationCleanerJobStart

Then at application shutdown stop the instance.

snippet: DeduplicationCleanerJobStop


### JSON headers


#### Serialization

Serialize a `Dictionary<string, string>` to a JSON string.

snippet: Serialize


#### Deserialization

Deserialize a JSON string to a `Dictionary<string, string>`.

snippet: Deserialize


### Copied header constants

Contains all the string constants copied from `NServiceBus.Headers`.

 
### Duplicated timestamp functionality

A copy of the [timestamp format methods](https://docs.particular.net/nservicebus/messaging/headers#timestamp-format) `ToWireFormattedString` and `ToUtcDateTime`. 


## ConnectionHelpers

The APIs of this extension target either a `SQLConnection` and `SQLTransaction`. Given that in configuration those values are often expressed as a connection string, `ConnectionHelpers` supports converting that string to a `SQLConnection` or `SQLTransaction`. It provides two methods `OpenConnection` and `BeginTransaction` with the effective implementation of those methods being:

snippet: ConnectionHelpers


## SqlServer.HttpPassthrough

SQL HTTP Passthrough provides a bridge between an HTTP stream (via JavaScript on a web page) and the [SQL Server transport](https://docs.particular.net/transports/sql/).

See [docs/http-passthrough.md](docs/http-passthrough.md).


## Icon

[Spear](https://thenounproject.com/term/spear/814550/) designed by [Aldric Rodríguez](https://thenounproject.com/aldricroib2/) from [The Noun Project](https://thenounproject.com/).