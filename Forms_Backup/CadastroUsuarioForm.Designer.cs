namespace HelpFastDesktop.Forms;

partial class CadastroUsuarioForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(500, 600);
        this.Name = "CadastroUsuarioForm";
        this.Text = "HELP FAST - Cadastro de Usuário";
        this.ResumeLayout(false);
    }
}
