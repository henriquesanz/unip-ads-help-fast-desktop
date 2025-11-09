using Microsoft.EntityFrameworkCore;
using HelpFastDesktop.Core.Models.Entities;

namespace HelpFastDesktop.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Cargo> Cargos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Chamado> Chamados { get; set; }
    public DbSet<FAQItem> Faqs { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<ChatIaResult> ChatIaResults { get; set; }
    public DbSet<HistoricoChamado> HistoricoChamados { get; set; }
    public DbSet<ConfiguracaoSistema> ConfiguracoesSistema { get; set; }
    public DbSet<Notificacao> Notificacoes { get; set; }
    public DbSet<LogAuditoria> LogsAuditoria { get; set; }
    public DbSet<InteracaoIA> InteracoesIA { get; set; }
    public DbSet<ConfiguracaoNotificacao> ConfiguracoesNotificacao { get; set; }
    public DbSet<Metrica> Metricas { get; set; }
    public DbSet<Relatorio> Relatorios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração do modelo Cargo
        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.ToTable("Cargos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
        });

        // Configuração do modelo Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Senha).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Telefone).HasMaxLength(15);

            // Índice único para email
            entity.HasIndex(e => e.Email).IsUnique();

            // Relacionamento com Cargo
            entity.HasOne(e => e.Cargo)
                  .WithMany(c => c.Usuarios)
                  .HasForeignKey(e => e.CargoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração do modelo Chamado
        modelBuilder.Entity<Chamado>(entity =>
        {
            entity.ToTable("Chamados");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Motivo).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DataAbertura).IsRequired();

            // Relacionamento com Cliente
            entity.HasOne(e => e.Cliente)
                  .WithMany()
                  .HasForeignKey(e => e.ClienteId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento com Tecnico
            entity.HasOne(e => e.Tecnico)
                  .WithMany()
                  .HasForeignKey(e => e.TecnicoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração do modelo FAQItem
        modelBuilder.Entity<FAQItem>(entity =>
        {
            entity.ToTable("Faqs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Pergunta).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Resposta).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.Ativo).IsRequired();
        });

        // Configuração do modelo Chat
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.ToTable("Chats");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Mensagem).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.Tipo).HasMaxLength(50);
            entity.Property(e => e.DataEnvio).IsRequired();

            // Relacionamentos
            entity.HasOne(e => e.Chamado)
                  .WithMany()
                  .HasForeignKey(e => e.ChamadoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Remetente)
                  .WithMany()
                  .HasForeignKey(e => e.RemetenteId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Destinatario)
                  .WithMany()
                  .HasForeignKey(e => e.DestinatarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração do modelo ChatIaResult
        modelBuilder.Entity<ChatIaResult>(entity =>
        {
            entity.ToTable("ChatIaResults");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ResultJson).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Chat)
                  .WithMany()
                  .HasForeignKey(e => e.ChatId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração do modelo HistoricoChamado
        modelBuilder.Entity<HistoricoChamado>(entity =>
        {
            entity.ToTable("HistoricoChamados");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Acao).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Data).IsRequired();

            entity.HasOne(e => e.Chamado)
                  .WithMany()
                  .HasForeignKey(e => e.ChamadoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração do modelo Notificacao
        modelBuilder.Entity<Notificacao>(entity =>
        {
            entity.ToTable("Notificacoes");
            entity.HasKey(e => e.Id);
            
            // Propriedades obrigatórias
            entity.Property(e => e.UsuarioId).IsRequired();
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Mensagem).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Prioridade).IsRequired().HasMaxLength(20).HasDefaultValue("Media");
            entity.Property(e => e.Lida).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.DataEnvio).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            
            // Propriedades opcionais
            entity.Property(e => e.DataLeitura).IsRequired(false);
            entity.Property(e => e.ChamadoId).IsRequired(false);
            entity.Property(e => e.Acao).HasMaxLength(100).IsRequired(false);

            // Relacionamento com Usuario
            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento com Chamado
            entity.HasOne(e => e.Chamado)
                  .WithMany()
                  .HasForeignKey(e => e.ChamadoId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índices para melhor performance
            entity.HasIndex(e => e.UsuarioId);
            entity.HasIndex(e => e.Lida);
            entity.HasIndex(e => e.DataEnvio);
            entity.HasIndex(e => e.ChamadoId);
        });

        // Configuração do modelo InteracaoIA
        modelBuilder.Entity<InteracaoIA>(entity =>
        {
            entity.ToTable("InteracoesIA");
            entity.HasKey(e => e.Id);
            
            // Propriedades obrigatórias
            entity.Property(e => e.UsuarioId).IsRequired();
            entity.Property(e => e.TipoInteracao).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DataInteracao).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            
            // Propriedades opcionais
            entity.Property(e => e.Pergunta).HasMaxLength(1000).IsRequired(false);
            entity.Property(e => e.Resposta).HasMaxLength(2000).IsRequired(false);
            entity.Property(e => e.Categoria).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.ProblemaResolvido).IsRequired(false);
            entity.Property(e => e.Satisfacao).IsRequired(false);
            entity.Property(e => e.TempoResposta).IsRequired(false);
            entity.Property(e => e.Confianca).IsRequired(false);
            entity.Property(e => e.ChamadoId).IsRequired(false);

            // Relacionamento com Usuario
            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento com Chamado
            entity.HasOne(e => e.Chamado)
                  .WithMany()
                  .HasForeignKey(e => e.ChamadoId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índices para melhor performance
            entity.HasIndex(e => e.UsuarioId);
            entity.HasIndex(e => e.TipoInteracao);
            entity.HasIndex(e => e.DataInteracao);
            entity.HasIndex(e => e.ChamadoId);
        });
    }
}

