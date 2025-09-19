namespace HelpFastDesktop.Forms;

partial class ChamadosAtribuidosForm
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
        this.ClientSize = new System.Drawing.Size(1000, 600);
        this.Name = "ChamadosAtribuidosForm";
        this.Text = "HELP FAST - Meus Chamados Atribuídos";
        this.ResumeLayout(false);
    }
}

