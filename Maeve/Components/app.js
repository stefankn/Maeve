import hljs from 'highlight.js/lib/core';

import go from 'highlight.js/lib/languages/go';
import javascript from 'highlight.js/lib/languages/javascript';
import bash from 'highlight.js/lib/languages/bash';
import json from 'highlight.js/lib/languages/json';
import csharp from 'highlight.js/lib/languages/csharp';
import xml from 'highlight.js/lib/languages/xml';
import swift from 'highlight.js/lib/languages/swift';
import python from 'highlight.js/lib/languages/python';
import dockerfile from 'highlight.js/lib/languages/dockerfile';
import css from 'highlight.js/lib/languages/css';

hljs.registerLanguage('go', go);
hljs.registerLanguage('javascript', javascript);
hljs.registerLanguage('bash', bash);
hljs.registerLanguage('json', json);
hljs.registerLanguage('csharp', csharp);
hljs.registerLanguage('xml', xml);
hljs.registerLanguage('swift', swift);
hljs.registerLanguage('python', python);
hljs.registerLanguage('dockerfile', dockerfile);
hljs.registerLanguage('css', css);

window.maeve = {
    scrollToBottom: function() {
        setTimeout(function() {
            window.scrollTo(0, document.body.scrollHeight);
        }, 100);
    },
    
    highlightCodeBlocks: function() {
        setTimeout(function() {
            console.log('highlightCodeBlocks');
            hljs.highlightAll();
        }, 100);
    }
}