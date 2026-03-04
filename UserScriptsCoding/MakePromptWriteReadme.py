import os, sys
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), "../ScriptUtils")))
from Core.PromptContextCollector.PromptContextCollector import PromptContextCollector

PromptContextCollector(
    files=[],
    directories=["Unity/gpu-rendering/Assets"],  # Wildcards and folders
    includes=["*.cs", "*.md"],
    ignores=[],
    template_path="UserScriptsCoding/TextTemplates/MakePromptWriteReadmeTemplate.txt",  # Relative to project root
    template_vars={"module_name": "GPU instance based rendering examples and tutorial"},
    output_path="UserScriptsCoding/Outputs/PromptWriteReadme.txt"
).run()