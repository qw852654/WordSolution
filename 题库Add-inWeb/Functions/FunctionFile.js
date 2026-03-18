// 每次加载新页面时都必须运行初始化函数。
Office.onReady(() => {
        // 如果你需要初始化，可以在此进行。
});

async function sampleFunction(event) {
try {
        await Word.run(async (context) => {
        // 在文档正文的末尾插入段落。
        const body = context.document.body;
        body.insertParagraph("You can use the Office Add-ins platform to build solutions that extend Office applications and interact with content in Office documents.", Word.InsertLocation.end);

        await context.sync();
        });
} catch (error) {
        console.error(error);
}

// 需要调用 event.completed。event.completed 会让平台知道处理已完成。
event.completed();
}
