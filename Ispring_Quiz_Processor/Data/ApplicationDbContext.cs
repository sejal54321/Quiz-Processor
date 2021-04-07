using Ispring_Quiz_Processor.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ispring_Quiz_Processor.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<SummaryDetails> SummaryDetails { get; set; }
        public virtual DbSet<QuestionDetails> QuestionDetails { get; set; }
        public virtual DbSet<XmlDetails> XmlDetails { get; set; }
    }
}
