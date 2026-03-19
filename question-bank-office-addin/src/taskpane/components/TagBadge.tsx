import * as React from "react";
import { makeStyles } from "@fluentui/react-components";

interface TagBadgeProps {
  文本: string;
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
});

export default function TagBadge(props: TagBadgeProps) {
  const styles = useStyles();
  return <span className={styles.badge}>{props.文本}</span>;
}
