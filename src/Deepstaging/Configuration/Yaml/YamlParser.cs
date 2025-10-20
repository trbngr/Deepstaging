using Microsoft.Extensions.Configuration;
using YamlDotNet.RepresentationModel;

namespace Deepstaging.Configuration.Yaml;

internal class YamlParser
{
    private YamlParser()
    {
    }

    private readonly IDictionary<string, string?> _data =
        new SortedDictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    private readonly Stack<string> _context = new();
    private string _currentPath = null!;

    public static IDictionary<string, string?> Parse(Stream input)
        => new YamlParser().ParseStream(input);

    private IDictionary<string, string?> ParseStream(Stream input)
    {
        _data.Clear();
        
        YamlStream yaml = new();
        yaml.Load(new StreamReader(input));
        
        if (yaml.Documents.Count <= 0) 
            return _data;

        VisitNode("", yaml.Documents[0].RootNode);

        return _data;
    }

    private void VisitNode(string context, YamlNode node)
    {
        switch (node)
        {
            case YamlScalarNode scalar:
                VisitYamlScalarNode(context, scalar);
                break;
            case YamlMappingNode mapping:
                VisitYamlMappingNode(context, mapping);
                break;
            case YamlSequenceNode seq:
                VisitYamlSequenceNode(context, seq);
                break;
        }
    }

    private void VisitYamlScalarNode(string context, YamlScalarNode node)
    {
        EnterContext(context);
        if (_data.ContainsKey(_currentPath))
            throw new Exception(_currentPath);
        _data[_currentPath] = node.Value!;
        ExitContext();
    }

    private void VisitYamlMappingNode(string context, YamlMappingNode node)
    {
        EnterContext(context);
        foreach (var yamlNode in node.Children)
        {
            context = ((YamlScalarNode)yamlNode.Key).Value!;
            VisitNode(context, yamlNode.Value);
        }

        ExitContext();
    }

    private void VisitYamlSequenceNode(string context, YamlSequenceNode node)
    {
        EnterContext(context);
        for (int i = 0; i < node.Children.Count; i++)
            VisitNode(i.ToString(), node.Children[i]);
        ExitContext();
    }

    private void EnterContext(string context)
    {
        if (string.IsNullOrWhiteSpace(context))
            return;

        _context.Push(context);
        _currentPath = ConfigurationPath.Combine(_context.Reverse());
    }

    private void ExitContext()
    {
        if (_context.Count == 0)
            return;

        _context.Pop();
        _currentPath = ConfigurationPath.Combine(_context.Reverse());
    }
}