using Skeleton.Abstractions;

namespace Skeleton.Service.Entities;

public record TestRequest(Id id, params string[] notes);
