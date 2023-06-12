using Skeleton.Abstractions;

namespace Skeleton.Service.Entities;

public record Test(Id id, params string[] notes);
