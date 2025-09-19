using Microsoft.EntityFrameworkCore;
using HelpFastDesktop.Models;

namespace HelpFastDesktop.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Chamado> Chamados { get; set; }
    public DbSet<Comentario> Comentarios { get; set; }
    public DbSet<Notificacao> Notificacoes { get; set; }
    public DbSet<FAQItem> FAQ { get; set; }
    public DbSet<InteracaoIA> InteracoesIA { get; set; }
    public DbSet<LogAuditoria> LogsAuditoria { get; set; }
    public DbSet<ConfiguracaoNotificacao> ConfiguracoesNotificacao { get; set; }
    public DbSet<Relatorio> Relatorios { get; set; }
    public DbSet<Metrica> Metricas { get; set; }
    public DbSet<ConfiguracaoSistema> ConfiguracoesSistema { get; set; }
    public DbSet<HistoricoNotificacoes> HistoricoNotificacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração do modelo Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Telefone).IsRequired().HasMaxLength(15);
            entity.Property(e => e.Senha).IsRequired().HasMaxLength(255);
            entity.Property(e => e.TipoUsuario).IsRequired().HasConversion<int>();
            entity.Property(e => e.DataCriacao).IsRequired();
            entity.Property(e => e.Ativo).IsRequired();

            // Índice único para email
            entity.HasIndex(e => e.Email).IsUnique();

            // Relacionamento auto-referencial para CriadoPor
            entity.HasOne(e => e.CriadoPor)
                  .WithMany()
                  .HasForeignKey(e => e.CriadoPorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração do modelo Chamado
        modelBuilder.Entity<Chamado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Prioridade).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Categoria).HasMaxLength(100);
            entity.Property(e => e.Subcategoria).HasMaxLength(100);
            entity.Property(e => e.ComentarioSatisfacao).HasMaxLength(500);
            entity.Property(e => e.DataCriacao).IsRequired();

            // Relacionamento com Usuario (criador do chamado)
            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento com Tecnico (atribuído)
            entity.HasOne(e => e.Tecnico)
                  .WithMany()
                  .HasForeignKey(e => e.TecnicoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração do modelo Comentario
        modelBuilder.Entity<Comentario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ComentarioTexto).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DataCriacao).IsRequired();

            entity.HasOne(e => e.Chamado)
                  .WithMany()
                  .HasForeignKey(e => e.ChamadoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração do modelo Notificacao
        modelBuilder.Entity<Notificacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Mensagem).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Prioridade).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Canal).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Acao).HasMaxLength(100);
            entity.Property(e => e.DataEnvio).IsRequired();

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Chamado)
                  .WithMany()
                  .HasForeignKey(e => e.ChamadoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração do modelo FAQItem
        modelBuilder.Entity<FAQItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Solucao).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Categoria).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Subcategoria).HasMaxLength(100);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.DataCriacao).IsRequired();

            entity.HasOne(e => e.CriadoPor)
                  .WithMany()
                  .HasForeignKey(e => e.CriadoPorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração do modelo InteracaoIA
        modelBuilder.Entity<InteracaoIA>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TipoInteracao).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Pergunta).HasMaxLength(1000);
            entity.Property(e => e.Resposta).HasMaxLength(2000);
            entity.Property(e => e.Categoria).HasMaxLength(100);
            entity.Property(e => e.DataInteracao).IsRequired();

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Chamado)
                  .WithMany()
                  .HasForeignKey(e => e.ChamadoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração do modelo LogAuditoria
        modelBuilder.Entity<LogAuditoria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Acao).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Tabela).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DadosAntigos).HasMaxLength(4000);
            entity.Property(e => e.DadosNovos).HasMaxLength(4000);
            entity.Property(e => e.IPAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.DataAcao).IsRequired();

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração do modelo ConfiguracaoNotificacao
        modelBuilder.Entity<ConfiguracaoNotificacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Frequencia).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DiasSemana).HasMaxLength(100);
            entity.Property(e => e.DataCriacao).IsRequired();

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UsuarioId).IsUnique();
        });

        // Configuração do modelo Relatorio
        modelBuilder.Entity<Relatorio>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.Parametros).HasMaxLength(4000);
            entity.Property(e => e.Frequencia).HasMaxLength(20);
            entity.Property(e => e.Destinatarios).HasMaxLength(4000);
            entity.Property(e => e.Formato).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DataCriacao).IsRequired();
            entity.Property(e => e.DataInicio).IsRequired();
            entity.Property(e => e.DataFim).IsRequired();

            entity.HasOne(e => e.CriadoPor)
                  .WithMany()
                  .HasForeignKey(e => e.CriadoPorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração do modelo Metrica
        modelBuilder.Entity<Metrica>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TipoMetrica).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ChamadosPorPrioridade).HasMaxLength(4000);
            entity.Property(e => e.ChamadosPorCategoria).HasMaxLength(4000);
            entity.Property(e => e.TempoResolucaoPorPrioridade).HasMaxLength(4000);
            entity.Property(e => e.DataMetrica).IsRequired();
            entity.Property(e => e.HoraMetrica).IsRequired();
            entity.Property(e => e.DataCriacao).IsRequired();
        });

        // Configuração do modelo ConfiguracaoSistema
        modelBuilder.Entity<ConfiguracaoSistema>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Chave).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Valor).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Descricao).HasMaxLength(200);
            entity.Property(e => e.DataAtualizacao).IsRequired();

            entity.HasIndex(e => e.Chave).IsUnique();

            entity.HasOne(e => e.AtualizadoPor)
                  .WithMany()
                  .HasForeignKey(e => e.AtualizadoPorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Dados iniciais para teste
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nome = "Administrador Master",
                Email = "admin@helpfast.com",
                Telefone = "(11) 99999-9999",
                Senha = "123456", // Em produção, deve ser hash
                TipoUsuario = UserRole.Administrador,
                DataCriacao = DateTime.Now,
                Ativo = true,
                CriadoPorId = null // Usuário master não foi criado por ninguém
            },
            new Usuario
            {
                Id = 2,
                Nome = "Técnico Padrão",
                Email = "tecnico@helpfast.com",
                Telefone = "(11) 88888-8888",
                Senha = "123456", // Em produção, deve ser hash
                TipoUsuario = UserRole.Tecnico,
                DataCriacao = DateTime.Now,
                Ativo = true,
                CriadoPorId = 1 // Criado pelo Administrador Master
            }
        );

        // Configuração do modelo HistoricoNotificacoes
        modelBuilder.Entity<HistoricoNotificacoes>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Canal).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DataEnvio).IsRequired();
            entity.Property(e => e.Provider).HasMaxLength(50);
            entity.Property(e => e.ProviderId).HasMaxLength(100);

            // Relacionamentos
            entity.HasOne(e => e.Notificacao)
                  .WithMany()
                  .HasForeignKey(e => e.NotificacaoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurações iniciais do sistema
        modelBuilder.Entity<ConfiguracaoSistema>().HasData(
            new ConfiguracaoSistema
            {
                Id = 1,
                Chave = "sistema_nome",
                Valor = "HelpFast",
                Tipo = "String",
                Descricao = "Nome do sistema",
                DataAtualizacao = DateTime.Now,
                AtualizadoPorId = 1
            },
            new ConfiguracaoSistema
            {
                Id = 2,
                Chave = "sistema_versao",
                Valor = "1.0.0",
                Tipo = "String",
                Descricao = "Versão atual do sistema",
                DataAtualizacao = DateTime.Now,
                AtualizadoPorId = 1
            },
            new ConfiguracaoSistema
            {
                Id = 3,
                Chave = "api_java_base_url",
                Valor = "https://helpfast-java-api.oraclecloud.com/api",
                Tipo = "String",
                Descricao = "URL base da API Java para IA e notificações",
                DataAtualizacao = DateTime.Now,
                AtualizadoPorId = 1
            },
            new ConfiguracaoSistema
            {
                Id = 4,
                Chave = "api_java_timeout",
                Valor = "30",
                Tipo = "Int",
                Descricao = "Timeout em segundos para requisições à API Java",
                DataAtualizacao = DateTime.Now,
                AtualizadoPorId = 1
            },
            new ConfiguracaoSistema
            {
                Id = 5,
                Chave = "api_java_retry_attempts",
                Valor = "3",
                Tipo = "Int",
                Descricao = "Número de tentativas para requisições à API Java",
                DataAtualizacao = DateTime.Now,
                AtualizadoPorId = 1
            },
            new ConfiguracaoSistema
            {
                Id = 6,
                Chave = "ia_categorizacao_ativa",
                Valor = "true",
                Tipo = "Boolean",
                Descricao = "Ativar categorização automática via IA",
                DataAtualizacao = DateTime.Now,
                AtualizadoPorId = 1
            },
            new ConfiguracaoSistema
            {
                Id = 7,
                Chave = "ia_atribuicao_ativa",
                Valor = "true",
                Tipo = "Boolean",
                Descricao = "Ativar atribuição automática via IA",
                DataAtualizacao = DateTime.Now,
                AtualizadoPorId = 1
            },
            new ConfiguracaoSistema
            {
                Id = 8,
                Chave = "notificacoes_email_ativas",
                Valor = "true",
                Tipo = "Boolean",
                Descricao = "Ativar notificações por email",
                DataAtualizacao = DateTime.Now,
                AtualizadoPorId = 1
            }
        );
    }
}

