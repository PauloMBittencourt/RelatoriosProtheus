using Microsoft.EntityFrameworkCore;
using RelatoriosProtheus.Models.Entities;

namespace RelatoriosProtheus.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Relatorios> Relatorios { get; set; }
        public DbSet<Parametros> Parametros { get; set; }
        public DbSet<Grupo> Grupos { get; set; }
        public DbSet<Funcionarios> Funcionarios { get; set; }
        public DbSet<Perfil> Perfis { get; set; }
        public DbSet<Perfis_Funcionarios> Perfis_Funcionarios { get; set; }
        public DbSet<FuncionarioGrupo> FuncionarioGrupos { get; set; }
        public DbSet<RelatorioGrupo> RelatorioGrupos { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Perfis_Funcionarios>(pf => {

                pf.ToTable("Perfis_Funcionarios");
                pf.HasKey(pf => new { pf.PerfilId, pf.FuncionarioId });

                pf.HasOne(pf => pf.PerfilFk)
                        .WithMany(p => p.FuncionariosFk)
                        .HasForeignKey(pf => pf.PerfilId)
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();

                pf.HasOne(pf => pf.FuncionarioFk)
                        .WithMany(f => f.Perfis)
                        .HasForeignKey(pf => pf.FuncionarioId)
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
            });
            
            modelBuilder.Entity<FuncionarioGrupo>(pf => {

                pf.ToTable("FuncionarioGrupo");
                pf.HasKey(pf => new { pf.GrupoId, pf.FuncionariosId});

                pf.HasOne(pf => pf.GrupoFk)
                        .WithMany(p => p.FuncionarioGrupos)
                        .HasForeignKey(pf => pf.GrupoId)
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();

                pf.HasOne(pf => pf.FuncionarioFk)
                        .WithMany(f => f.FuncionarioGrupos)
                        .HasForeignKey(pf => pf.FuncionariosId)
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
            });
            
            modelBuilder.Entity<RelatorioGrupo>(pf => {

                pf.ToTable("RelatorioGrupo");
                pf.HasKey(pf => new { pf.GrupoId, pf.RelatoriosId});

                pf.HasOne(pf => pf.GrupoFk)
                        .WithMany(p => p.RelatorioGrupos)
                        .HasForeignKey(pf => pf.GrupoId)
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();

                pf.HasOne(pf => pf.RelatoriosFk)
                        .WithMany(f => f.RelatorioGrupos)
                        .HasForeignKey(pf => pf.RelatoriosId)
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
            });


            modelBuilder.Entity<Parametros>()
                        .HasKey(p => p.Id);

            modelBuilder.Entity<Relatorios>()
                        .HasKey(r => r.Id);

            modelBuilder.Entity<Relatorios>()
                        .Property(r => r.Id)
                        .ValueGeneratedOnAdd();

            modelBuilder.Entity<Relatorios>()
                .HasMany(r => r.Parametros)
                .WithOne(p => p.Relatorio)
                .HasForeignKey(p => p.RelatorioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
