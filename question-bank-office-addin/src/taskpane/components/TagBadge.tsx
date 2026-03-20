import * as React from "react";
import { makeStyles } from "@fluentui/react-components";

interface TagBadgeProps {
  文本: string;
  强调?: boolean;
  onClick?: () => void;
}

const useStyles = makeStyles({
  badge: {
    display: "inline-flex",
    alignItems: "center",
    padding: "3px 6px",
    borderRadius: "999px",
    border: "1px solid #d2b487",
    backgroundColor: "#fbf4e4",
    color: "#5f4a22",
    fontSize: "11px",
    lineHeight: "14px",
    whiteSpace: "nowrap",
  },
  emphasizedBadge: {
    border: "1px solid #b8860b",
    backgroundColor: "#f3c86a",
    color: "#3b2a00",
  },
  interactiveBadge: {
    cursor: "pointer",
    transition: "transform 120ms ease, box-shadow 120ms ease, background-color 120ms ease, border-color 120ms ease",
    ":hover": {
      border: "1px solid #c08a1d",
      backgroundColor: "#fff0cc",
      boxShadow: "0 2px 8px rgba(90, 65, 20, 0.12)",
      transform: "translateY(-1px)",
    },
    ":focus-visible": {
      outline: "2px solid #d79b27",
      outlineOffset: "2px",
    },
    ":active": {
      transform: "translateY(0)",
      boxShadow: "none",
    },
  },
});

export default function TagBadge(props: TagBadgeProps) {
  const styles = useStyles();
  const className = [styles.badge, props.强调 ? styles.emphasizedBadge : "", props.onClick ? styles.interactiveBadge : ""]
    .filter(Boolean)
    .join(" ");

  if (!props.onClick) {
    return <span className={className}>{props.文本}</span>;
  }

  return (
    <span
      className={className}
      role="button"
      tabIndex={0}
      onClick={props.onClick}
      onKeyDown={(事件) => {
        if (事件.key === "Enter" || 事件.key === " ") {
          事件.preventDefault();
          props.onClick?.();
        }
      }}
    >
      {props.文本}
    </span>
  );
}
