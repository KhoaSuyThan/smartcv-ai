import React from 'react';

class RichInput extends React.Component {
  constructor(props) {
    super(props);
    this.elRef = React.createRef();
    this.handleInput = this.handleInput.bind(this);
    this.handleKeyDown = this.handleKeyDown.bind(this);
  }

  componentDidMount() {
    this.updateEmptyClass();
  }

  shouldComponentUpdate(nextProps) {
    const el = this.elRef.current;
    if (!el) return true;

    // We only need to re-render if the incoming value is different from the actual DOM's innerHTML
    // Or if other key props have changed
    const valueHasChanged = el.innerHTML !== (nextProps.value || '');
    const isTextareaChanged = this.props.isTextarea !== nextProps.isTextarea;
    const placeholderChanged = this.props.placeholder !== nextProps.placeholder;
    const styleChanged = JSON.stringify(this.props.style) !== JSON.stringify(nextProps.style);

    return valueHasChanged || isTextareaChanged || placeholderChanged || styleChanged;
  }

  componentDidUpdate() {
    const el = this.elRef.current;
    if (el && el.innerHTML !== (this.props.value || '')) {
      el.innerHTML = this.props.value || '';
    }
    this.updateEmptyClass();
  }

  updateEmptyClass() {
    const el = this.elRef.current;
    if (el) {
      const html = el.innerHTML;
      const isEmpty = !html || html === '<br>' || html === '<div><br></div>' || html === '<p><br></p>' || html.replace(/&nbsp;/g, '').trim() === '';
      if (isEmpty) {
        el.classList.add('is-empty');
      } else {
        el.classList.remove('is-empty');
      }
    }
  }

  handleInput(e) {
    const html = e.target.innerHTML;
    this.updateEmptyClass();
    if (this.props.onChange) {
      this.props.onChange(html);
    }
  }

  handleKeyDown(e) {
    // If it's not a textarea, prevent the Enter key from adding a newline
    if (!this.props.isTextarea && e.key === 'Enter') {
      e.preventDefault();
    }
  }

  render() {
    const { value, onChange, placeholder, style, className, isTextarea, ...props } = this.props;

    const mergedStyle = {
      width: '100%',
      padding: '10px 14px',
      border: '1px solid #cbd5e1',
      borderRadius: '8px',
      boxSizing: 'border-box',
      fontSize: '13px',
      color: '#0f172a',
      backgroundColor: '#f8fafc',
      transition: 'all 0.2s',
      outline: 'none',
      overflowY: isTextarea ? 'auto' : 'hidden',
      whiteSpace: isTextarea ? 'pre-wrap' : 'nowrap',
      minHeight: isTextarea ? '80px' : '38px',
      cursor: 'text',
      textAlign: 'left',
      display: 'block',
      position: 'relative',
      ...style
    };

    return (
      <div
        ref={this.elRef}
        contentEditable
        dangerouslySetInnerHTML={{ __html: value || '' }}
        onInput={this.handleInput}
        onKeyDown={this.handleKeyDown}
        style={mergedStyle}
        className={`${className || ''} rich-text-editor`}
        placeholder={placeholder}
        {...props}
      />
    );
  }
}

export default RichInput;
