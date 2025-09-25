// define a custom element that includes a normal input element.
// The custom element will be used to demonstrate how to interact with the input element.

class CustomInput extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.input = document.createElement('input');
        this.input.setAttribute('type', 'text');
        this.shadowRoot.appendChild(this.input);
    }

    focus() { this.input.focus(); }

    get value() { return this.input.value; }
}
customElements.define('custom-input', CustomInput);

class CustomTextArea extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.textArea = document.createElement('textarea');
        this.shadowRoot.appendChild(this.textArea);
    }

    focus() { this.textArea.focus(); }

    get value() { return this.textArea.value; }
}
customElements.define('custom-textarea', CustomTextArea);

class CustomCheckbox extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.checkbox = document.createElement('input');
        this.checkbox.setAttribute('type', 'checkbox');
        this.shadowRoot.appendChild(this.checkbox);
    }

    focus() { this.checkbox.focus(); }

    get checked() { return this.checkbox.checked; }
}
customElements.define('custom-checkbox', CustomCheckbox);

class CustomSelect extends HTMLElement {

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.select = document.createElement('select');
        this.shadowRoot.appendChild(this.select);
    }

    connectedCallback() {
        const options = this.querySelectorAll('option');
        options.forEach(option => {
            const selectOption = document.createElement('option');
            selectOption.textContent = option.textContent;
            selectOption.value = option.value;
            this.select.appendChild(selectOption);
        });
    }

    focus() { this.select.focus(); }

    get value() { return this.select.value; }
}
customElements.define('custom-select', CustomSelect);

class CustomRadioGroup extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        const radioGroup = document.createElement('div');
        this.shadowRoot.appendChild(radioGroup);
        for (let i = 0; i < 3; i++) {
            const radio = document.createElement('input');
            radio.setAttribute('type', 'radio');
            radio.setAttribute('name', 'custom-radio-group');
            radio.setAttribute('value', i);
            if (i == 0) radio.setAttribute('checked', true);
            radioGroup.appendChild(radio);
        }
    }

    focus() { this.shadowRoot.querySelector('input').focus(); }

    get value() { return this.shadowRoot.querySelector('input:checked').value; }
}
customElements.define('custom-radio-group', CustomRadioGroup);

class CustomButton extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.button = document.createElement('button');
        this.button.textContent = 'Click me';
        this.shadowRoot.appendChild(this.button);
    }

    connectedCallback() {
        const text = this.textContent;
        this.button.textContent = text;
    }

    focus() { this.button.focus(); }
}
customElements.define('custom-button', CustomButton);

class CustomInputButton extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.input = document.createElement('input');
        this.input.setAttribute('type', 'button');
        this.shadowRoot.appendChild(this.input);
    }

    connectedCallback() {
        const text = this.textContent;
        this.input.value = text;
    }

    focus() { this.input.focus(); }
}
customElements.define('custom-input-button', CustomInputButton);