const illegalFileNameCharacters = /[<>:"/\\|?*\u0000-\u001F]/g;
const reservedWindowsFileName = /^(con|prn|aux|nul|com[1-9]|lpt[1-9])(\..*)?$/i;
const maxBaseFileNameLength = 80;

export function sanitizeFileName(value: string): string {
    const sanitized = value
        .replace(illegalFileNameCharacters, '')
        .trim()
        .replace(/[. ]+$/g, '');

    if (!sanitized || reservedWindowsFileName.test(sanitized)) {
        return 'expression';
    }

    return Array.from(sanitized)
        .slice(0, maxBaseFileNameLength)
        .join('');
}
