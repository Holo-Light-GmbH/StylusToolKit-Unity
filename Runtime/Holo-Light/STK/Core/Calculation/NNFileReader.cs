using HoloLight.STK.Core.Tracker;
using System.IO;
using System.Globalization;

namespace HoloLight.STK.Core
{
    public static class NNFileReader
    {
        public static NeuralNetworkData GetData(string filepath)
        {
            StreamReader _fileStream;
            var neuralNetworkData = new NeuralNetworkData();
            _fileStream = File.OpenText(filepath);
            var line = _fileStream.ReadLine();
            string[] values = line.Split(';');
            neuralNetworkData.NInputs = int.Parse(values[0]);
            neuralNetworkData.NLayers = int.Parse(values[1]);
            neuralNetworkData.NNeurons = int.Parse(values[2]);
            neuralNetworkData.NOutputs = int.Parse(values[3]);

            neuralNetworkData.Weights = new Matrix.Matrix[neuralNetworkData.NLayers];
            neuralNetworkData.Biases = new Matrix.Matrix[neuralNetworkData.NLayers];
            neuralNetworkData.WeightsIn = new Matrix.Matrix(neuralNetworkData.NInputs, neuralNetworkData.NNeurons);
            neuralNetworkData.BiasesIn = new Matrix.Matrix(1, neuralNetworkData.NNeurons);
            neuralNetworkData.WeightsOut = new Matrix.Matrix(neuralNetworkData.NNeurons, neuralNetworkData.NOutputs);
            neuralNetworkData.BiasesOut = new Matrix.Matrix(1, neuralNetworkData.NOutputs);

            for (int layerIndex = 0; layerIndex < neuralNetworkData.NLayers; layerIndex++)
            {
                neuralNetworkData.Weights[layerIndex] = new Matrix.Matrix(neuralNetworkData.NNeurons, neuralNetworkData.NNeurons);
                neuralNetworkData.Biases[layerIndex] = new Matrix.Matrix(1, neuralNetworkData.NNeurons);
            }
            line = _fileStream.ReadLine();
            for (int inputIndex = 0; inputIndex < neuralNetworkData.NInputs; inputIndex++)
            {
                line = _fileStream.ReadLine();
                values = line.Split(';');
                for (int neuronIndex = 0; neuronIndex < neuralNetworkData.NNeurons; neuronIndex++)
                {
                    var a = float.Parse(values[neuronIndex], CultureInfo.InvariantCulture);
                    neuralNetworkData.WeightsIn.SetElement(inputIndex, neuronIndex, a);
                }
            }
            line = _fileStream.ReadLine();
            line = _fileStream.ReadLine();
            for (int layerIndex = 0; layerIndex < neuralNetworkData.NLayers; layerIndex++)
            {
                for (int neuronRowIndex = 0; neuronRowIndex < neuralNetworkData.NNeurons; neuronRowIndex++)
                {
                    line = _fileStream.ReadLine();
                    values = line.Split(';');
                    for (int neuronsColumnIndex = 0; neuronsColumnIndex < neuralNetworkData.NNeurons; neuronsColumnIndex++)
                    {
                        neuralNetworkData.Weights[layerIndex].SetElement(neuronRowIndex, neuronsColumnIndex,
                            float.Parse(values[neuronsColumnIndex], CultureInfo.InvariantCulture));
                    }
                }
                line = _fileStream.ReadLine();
            }
            line = _fileStream.ReadLine();
            for (int neuronIndex = 0; neuronIndex < neuralNetworkData.NNeurons; neuronIndex++)
            {
                line = _fileStream.ReadLine();
                values = line.Split(';');
                for (int outputIndex = 0; outputIndex < neuralNetworkData.NOutputs; outputIndex++)
                {
                    neuralNetworkData.WeightsOut.SetElement(neuronIndex, outputIndex, float.Parse(values[outputIndex], CultureInfo.InvariantCulture));
                }
            }
            line = _fileStream.ReadLine();
            line = _fileStream.ReadLine();
            for (int layerIndex = 0; layerIndex < neuralNetworkData.NLayers; layerIndex++)
            {
                line = _fileStream.ReadLine();
                values = line.Split(';');
                for (int neuronIndex = 0; neuronIndex < neuralNetworkData.NNeurons; neuronIndex++)
                {
                    neuralNetworkData.Biases[layerIndex].SetElement(0, neuronIndex, float.Parse(values[neuronIndex], CultureInfo.InvariantCulture));
                }
            }
            line = _fileStream.ReadLine();
            line = _fileStream.ReadLine();
            line = _fileStream.ReadLine();
            values = line.Split(';');
            for (int neuronIndex = 0; neuronIndex < neuralNetworkData.NNeurons; neuronIndex++)
            {
                neuralNetworkData.BiasesIn.SetElement(0, neuronIndex, float.Parse(values[neuronIndex], CultureInfo.InvariantCulture));
            }
            line = _fileStream.ReadLine();
            line = _fileStream.ReadLine();
            line = _fileStream.ReadLine();
            values = line.Split(';');
            for (int outputIndex = 0; outputIndex < neuralNetworkData.NOutputs; outputIndex++)
            {
                neuralNetworkData.BiasesOut.SetElement(0, outputIndex, float.Parse(values[outputIndex], CultureInfo.InvariantCulture));
            }
            return neuralNetworkData;
        }
    }
}