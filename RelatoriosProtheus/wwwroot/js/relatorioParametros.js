// relatorioParametros.js
class RelatorioParametros {
    constructor() {
        this.acaoRelatorio = null;
        this.relatorioIdAtual = null;
    }

    abrirModal(relatorioId, acao) {
        this.acaoRelatorio = acao;
        this.relatorioIdAtual = relatorioId;

        fetch(`/Relatorios/CarregarModalParametros/${relatorioId}`)
            .then(response => response.text())
            .then(html => {
                document.body.insertAdjacentHTML('beforeend', html);
                const modal = new bootstrap.Modal(document.getElementById('relatorioParametrosModal'));
                modal.show();

                this.carregarParametros(relatorioId);
            })
            .catch(error => this.tratarErro('modal', error));
    }

    carregarParametros(relatorioId) {
        fetch(`/Relatorios/ObterParametros/${relatorioId}`)
            .then(response => response.text())
            .then(parametrosStr => {
                const container = document.getElementById('parametrosContainer');
                container.innerHTML = '';

                parametrosStr.split(',')
                    .map(param => this.processarParametro(param))
                    .forEach(({ chave, valor }) => this.criarCampo(container, chave, valor));
            })
            .catch(error => this.tratarErro('parâmetros', error));
    }

    processarParametro(param) {
        const [chaveRaw, valorRaw] = param.split(':').map(p => p.trim());
        return {
            chave: chaveRaw.replace(/["']/g, ''),
            valor: valorRaw ? valorRaw.replace(/["']/g, '') : ''
        };
    }

    criarCampo(container, chave, valor) {
        const div = document.createElement('div');
        div.className = 'mb-3';
        div.innerHTML = `
      <label class="form-label">${chave}</label>
      <input type="text" class="form-control" 
             name="${chave}" 
             value="${valor}" 
             placeholder="${valor ? '' : 'Insira o valor'}"/>
    `;
        container.appendChild(div);
    }

    tratarErro(tipo, error) {
        console.error(`Erro ao carregar ${tipo}:`, error);
        alert(`Erro ao carregar ${tipo}. Consulte o console para detalhes.`);
    }
}

// Inicialização global (opcional)
window.relatorioParametros = new RelatorioParametros();