# Records
Immutable record types for c#

# Features
## Constructor generating
If we declare class User like this:
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
