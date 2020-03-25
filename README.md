# Records

[![GitHub Actions CI status](https://github.com/sobolev88/Records/workflows/.NET%20Core/badge.svg?branch=master)](https://github.com/sobolev88/Records/actions?query=workflow%3A%22.NET+Core%22+branch%3Amaster)
[![NuGet package](https://img.shields.io/nuget/v/Records.svg)](https://www.nuget.org/packages/Records)

Immutable record types for c#

# Features
## Constructor generating
If we declare class `User` like this:
``` csharp
    [Record]
    public partial class User
    {
        public Guid Id { get; } = Guid.NewGuid();

        public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

        public string FirstName { get; }

        public string LastName { get; }

        public string? MiddleName { get; }

        public DateTime? BirthDate { get; }
    }
```
the following constructor would be generated:
``` csharp
        public User(string firstName,
                    string lastName,
                    Guid? id = default(Guid?),
                    DateTimeOffset? createdAt = default(DateTimeOffset?),
                    string? middleName = default(string?),
                    DateTime? birthDate = default(DateTime?))
        {
            FirstName = firstName;
            LastName = lastName;
            if (id != null)
                Id = id.Value;
            if (createdAt != null)
                CreatedAt = createdAt.Value;
            MiddleName = middleName;
            BirthDate = birthDate;
        }
```
## With methods generating
If we specify `with` parameter in `Record` attribute like this:
``` csharp
    [Record(with: true)]
    public partial class User
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string FirstName { get; }

        public string LastName { get; }

        public string? MiddleName { get; }
    }
```
the following methods and constructor would be generated:
``` csharp
        public User(string firstName,
                    string lastName,
                    Guid? id = default(Guid?),
                    string? middleName = default(string?))
        {
            FirstName = firstName;
            LastName = lastName;
            if (id != null)
                Id = id.Value;
            MiddleName = middleName;
        }

        public User WithFirstName(string firstName)
        {
            return new User(firstName, LastName, Id, MiddleName);
        }

        public User WithLastName(string lastName)
        {
            return new User(FirstName, lastName, Id, MiddleName);
        }

        public User WithId(Guid id)
        {
            return new User(FirstName, LastName, id, MiddleName);
        }

        public User WithMiddleName(string? middleName)
        {
            return new User(FirstName, LastName, Id, middleName);
        }
```
