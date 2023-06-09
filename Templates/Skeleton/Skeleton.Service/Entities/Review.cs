using Skeleton.Abstractions;

namespace Skeleton.Service.Entities;

public record Review(Id id, params string[] notes);
