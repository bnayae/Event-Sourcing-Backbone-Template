using Company.Placeholder.Abstractions;

namespace Company.Placeholder.Service.Entities;

public record Review(Id id, params string[] notes);
