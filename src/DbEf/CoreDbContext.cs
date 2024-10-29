using Microsoft.EntityFrameworkCore;

namespace DbEf;

public class CoreDbContext(DbContextOptions<CoreDbContext> options)
    : DbContext(options);