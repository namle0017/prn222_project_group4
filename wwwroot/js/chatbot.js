(function () {
  'use strict';

  var panel = document.getElementById('fap-chatbot-panel');
  var launcher = document.getElementById('fap-chatbot-launcher');
  var closeButton = document.getElementById('fap-chatbot-close');
  var form = document.getElementById('fap-chatbot-form');
  var input = document.getElementById('fap-chatbot-input');
  var sendButton = document.getElementById('fap-chatbot-send');
  var messages = document.getElementById('fap-chatbot-messages');

  if (!panel || !launcher || !closeButton || !form || !input || !sendButton || !messages) return;

  function scrollMessages() {
    messages.scrollTop = messages.scrollHeight;
  }

  function setOpen(open) {
    panel.hidden = !open;
    launcher.setAttribute('aria-expanded', String(open));
    if (open) {
      window.setTimeout(function () { input.focus(); }, 0);
      scrollMessages();
    } else {
      launcher.focus();
    }
  }

  function addMessage(text, isUser, actionLabel, actionUrl) {
    var item = document.createElement('div');
    item.className = 'fap-chatbot__message ' + (isUser ? 'fap-chatbot__message--user' : 'fap-chatbot__message--assistant');

    if (!isUser) {
      var avatar = document.createElement('span');
      avatar.className = 'fap-chatbot__avatar';
      avatar.setAttribute('aria-hidden', 'true');
      avatar.innerHTML = '<i class="bi bi-stars"></i>';
      item.appendChild(avatar);
    }

    var body = document.createElement('div');
    var paragraph = document.createElement('p');
    paragraph.textContent = text;
    body.appendChild(paragraph);

    if (!isUser && actionLabel && actionUrl && actionUrl.charAt(0) === '/' && actionUrl.indexOf('//') !== 0) {
      var action = document.createElement('a');
      action.className = 'fap-chatbot__action';
      action.href = actionUrl;
      action.innerHTML = '<i class="bi bi-arrow-up-right"></i>';
      action.appendChild(document.createTextNode(actionLabel));
      body.appendChild(action);
    }

    item.appendChild(body);
    messages.appendChild(item);
    scrollMessages();
  }

  function addTyping() {
    var item = document.createElement('div');
    item.id = 'fap-chatbot-typing';
    item.className = 'fap-chatbot__message fap-chatbot__message--assistant';
    item.innerHTML = '<span class="fap-chatbot__avatar" aria-hidden="true"><i class="bi bi-stars"></i></span><div class="fap-chatbot__typing" aria-label="Trợ lý đang trả lời"><span></span><span></span><span></span></div>';
    messages.appendChild(item);
    scrollMessages();
  }

  function removeTyping() {
    var typing = document.getElementById('fap-chatbot-typing');
    if (typing) typing.remove();
  }

  async function ask(question) {
    var message = (question || input.value).trim();
    if (!message || sendButton.disabled) return;

    setOpen(true);
    addMessage(message, true);
    input.value = '';
    input.style.height = '';
    sendButton.disabled = true;
    input.disabled = true;
    addTyping();

    try {
      var data = new FormData(form);
      data.set('Message', message);
      var response = await fetch(form.action, {
        method: 'POST',
        body: data,
        credentials: 'same-origin',
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
      });
      var payload;
      try {
        payload = await response.json();
      } catch (parseError) {
        payload = null;
      }

      if (response.status === 401) {
        addMessage('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.', false);
      } else if (!payload || !payload.answer) {
        addMessage('Chatbot tạm thời không thể trả lời. Vui lòng thử lại sau.', false);
      } else {
        addMessage(payload.answer, false, payload.suggestedActionLabel, payload.suggestedActionUrl);
      }
    } catch (requestError) {
      addMessage('Chatbot tạm thời không kết nối được. Vui lòng thử lại sau.', false);
    } finally {
      removeTyping();
      sendButton.disabled = false;
      input.disabled = false;
      input.focus();
    }
  }

  launcher.addEventListener('click', function () { setOpen(panel.hidden); });
  closeButton.addEventListener('click', function () { setOpen(false); });
  form.addEventListener('submit', function (event) {
    event.preventDefault();
    ask();
  });
  input.addEventListener('keydown', function (event) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      ask();
    }
  });
  input.addEventListener('input', function () {
    input.style.height = 'auto';
    input.style.height = Math.min(input.scrollHeight, 92) + 'px';
  });
  document.querySelectorAll('[data-chat-question]').forEach(function (button) {
    button.addEventListener('click', function () {
      ask(button.getAttribute('data-chat-question'));
    });
  });
  document.addEventListener('keydown', function (event) {
    if (event.key === 'Escape' && !panel.hidden) setOpen(false);
  });
})();
