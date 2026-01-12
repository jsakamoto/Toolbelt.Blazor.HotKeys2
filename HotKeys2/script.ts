
// Constants

const enum Exclude {
    None = 0,
    InputText = 0b0001,
    InputNonText = 0b0010,
    TextArea = 0b0100,
    ContentEditable = 0b1000
}

const enum ModCodes {
    None = 0,
    Shift = 0x01,
    Control = 0x02,
    Alt = 0x04,
    Meta = 0x08
}

const enum HotKeyMode {
    ByKey,
    ByCode
}

const doc = document;

const OnKeyDownMethodName = "OnKeyDown";

const NonTextInputTypes = ["button", "checkbox", "color", "file", "image", "radio", "range", "reset", "submit",];

const InputTageName = "INPUT";

const keydown = "keydown";

class HotkeyEntry {

    constructor(
        private dotNetObj: any,
        public mode: HotKeyMode,
        public modifiers: ModCodes,
        public keyEntry: string,
        public exclude: Exclude,
        public excludeSelector: string,
        public state: HotKeyEntryState
    ) { }

    public action(): void {
        this.dotNetObj.invokeMethodAsync('InvokeAction');
    }
}

// Static Functions

const addKeyDownEventListener = (listener: (ev: KeyboardEvent) => void) => doc.addEventListener(keydown, listener);

const removeKeyDownEventListener = (listener: (ev: KeyboardEvent) => void) => doc.removeEventListener(keydown, listener);

const convertToKeyName = (ev: KeyboardEvent): string => {
    const convertToKeyNameMap: { [key: string]: string } = {
        "OS": "Meta",
        "Decimal": "Period",
    };
    return convertToKeyNameMap[ev.key] || ev.key;
}

const startsWith = (str: string, prefix: string): boolean => str.startsWith(prefix);

const isExcludeTarget = (entry: HotkeyEntry, targetElement: HTMLElement, tagName: string, type: string | null): boolean => {

    if ((entry.exclude & Exclude.InputText) !== 0) {
        if (tagName === InputTageName && NonTextInputTypes.every(t => t !== type)) return true;
    }
    if ((entry.exclude & Exclude.InputNonText) !== 0) {
        if (tagName === InputTageName && NonTextInputTypes.some(t => t === type)) return true;
    }
    if ((entry.exclude & Exclude.TextArea) !== 0) {
        if (tagName === "TEXTAREA") return true;
    }
    if ((entry.exclude & Exclude.ContentEditable) !== 0) {
        if (targetElement.isContentEditable) return true;
    }

    if (entry.excludeSelector !== '' && targetElement.matches(entry.excludeSelector)) return true;

    return false;
}

type KeyEventTarget = [HTMLElement, string, string | null];
type KeyEventHandler = (modifiers: ModCodes, key: string, code: string, targets: KeyEventTarget[]) => boolean;
type HotKeyEntryState = { disabled: boolean, preventDefault: boolean };

const createKeydownHandler = (callback: KeyEventHandler) => {
    return (ev: KeyboardEvent) => {
        if (typeof (ev["altKey"]) === 'undefined') return;

        // Check if NumLock is active and adjust shiftKey accordingly for Numpad keys (Issue #32 (https://github.com/jsakamoto/Toolbelt.Blazor.HotKeys2/issues/32))
        const numLock = ev.getModifierState('NumLock');
        const shiftKey = (numLock && ev.code.match(/^Numpad([0-9]|Decimal)$/) && !ev.key.match(/^[0-9\.]$/)) ? true : ev.shiftKey;

        const modifiers =
            (shiftKey ? ModCodes.Shift : 0) +
            (ev.ctrlKey ? ModCodes.Control : 0) +
            (ev.altKey ? ModCodes.Alt : 0) +
            (ev.metaKey ? ModCodes.Meta : 0);
        const key = convertToKeyName(ev);
        const code = ev.code;

        const targets = [ev.target as HTMLElement, ev.composedPath()[0] as HTMLElement | undefined]
            .filter(e => e)
            .map<KeyEventTarget>(e => [e!, e!.tagName, e!.getAttribute('type')]);

        const preventDefault = callback(modifiers, key, code, targets);
        if (preventDefault) ev.preventDefault();
    }
}

export const createContext = () => {
    let idSeq: number = 0;
    const hotKeyEntries = new Map<number, HotkeyEntry>();

    const onKeyDown: KeyEventHandler = (modifiers, key, code, targets) => {
        let preventDefault = false;

        hotKeyEntries.forEach(entry => {

            if (!entry.state.disabled) {
                const byCode = entry.mode === HotKeyMode.ByCode;
                const eventKeyEntry = byCode ? code : key;
                const keyEntry = entry.keyEntry;

                if (keyEntry !== eventKeyEntry) return;

                const eventModkeys = byCode ? modifiers : (modifiers & (0xffff ^ ModCodes.Shift));
                let entryModKeys = byCode ? entry.modifiers : (entry.modifiers & (0xffff ^ ModCodes.Shift));
                if (startsWith(keyEntry, "Shift") && byCode) entryModKeys |= ModCodes.Shift;
                if (startsWith(keyEntry, "Control")) entryModKeys |= ModCodes.Control;
                if (startsWith(keyEntry, "Alt")) entryModKeys |= ModCodes.Alt;
                if (startsWith(keyEntry, "Meta")) entryModKeys |= ModCodes.Meta;
                if (eventModkeys !== entryModKeys) return;

                if (targets.some(([targetElement, tagName, type]) => isExcludeTarget(entry, targetElement, tagName, type))) return;

                preventDefault = preventDefault || entry.state.preventDefault;
                entry.action();
            }
        });

        return preventDefault;
    }

    const keydownHandler = createKeydownHandler(onKeyDown);

    addKeyDownEventListener(keydownHandler);

    return {
        register: (dotNetObj: any, mode: HotKeyMode, modifiers: ModCodes, keyEntry: string, exclude: Exclude, excludeSelector: string, initialState: HotKeyEntryState): number => {
            const id = idSeq++;
            const hotKeyEntry = new HotkeyEntry(dotNetObj, mode, modifiers, keyEntry, exclude, excludeSelector, initialState);
            hotKeyEntries.set(id, hotKeyEntry);
            return id;
        },

        update: (id: number, state: HotKeyEntryState): void => {
            const hotkeyEntry = hotKeyEntries.get(id);
            if (!hotkeyEntry) return;
            hotkeyEntry.state = state;
        },

        unregister: (id: number): void => {
            if (id === -1) return;
            hotKeyEntries.delete(id);
        },

        dispose: (): void => { removeKeyDownEventListener(keydownHandler); }
    };
}

export const handleKeyEvent = (hotKeysWrapper: any, isWasm: boolean) => {

    const onKeyDown: KeyEventHandler = (modifiers, key, code, targets: KeyEventTarget[]) => {
        const [, tagName, type] = targets[0];
        if (isWasm) {
            return hotKeysWrapper.invokeMethod(OnKeyDownMethodName, modifiers, tagName, type, key, code);
        } else {
            hotKeysWrapper.invokeMethodAsync(OnKeyDownMethodName, modifiers, tagName, type, key, code);
            return false;
        }
    }

    const keydownHandler = createKeydownHandler(onKeyDown);

    addKeyDownEventListener(keydownHandler);

    return {
        dispose: () => { removeKeyDownEventListener(keydownHandler); }
    };
}
